using Catan.Proxy;
using System;
using System.Collections.ObjectModel;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PublicDataCtrl : UserControl
    {
        private static void PlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PublicDataCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetPlayer(depPropValue);
        }

        private static void RollOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PublicDataCtrl;
            depPropClass?.SetRollOrientation((TileOrientation)e.OldValue, (TileOrientation)e.NewValue);
        }

        private void OnFlipRollGrid(object sender, RoutedEventArgs e)
        {
            Player.GameData.RollOrientation = (Player.GameData.RollOrientation == TileOrientation.FaceDown) ? TileOrientation.FaceUp : TileOrientation.FaceDown;
        }

        private async void Picture_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            ellipse.IsTapEnabled = false;
            try
            {
                Pointer ptr = e.Pointer;
                if (ptr.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                {
                    PointerPoint ptrPt = e.GetCurrentPoint(this);
                    if (ptrPt.Properties.IsRightButtonPressed)
                    {
                        MainPage.Current.TheHuman = Player;
                        MainPage.Current.CurrentPlayer = Player;
                        return;
                    }
                }

                PlayerModel player = ((Ellipse)sender).Tag as PlayerModel;

                if (await StaticHelpers.AskUserYesNoQuestion($"Let {player.PlayerName} go first?", "Yes", "No"))
                {
                    //  await MainPage.Current.SetFirst(player); //manipulates the shared PlayingPlayers list, but also does logging and other book keeping.
                }
            }
            finally
            {
                ellipse.IsTapEnabled = true;
            }
        }

        private void SetPlayer(PlayerModel value)
        {
            if (value == null) return;

            this.TraceMessage($"Added Player {value}");
        }

        private void SetRollOrientation(TileOrientation oldValue, TileOrientation newValue)
        {
            if (Player == null) return;
            if (oldValue == newValue) return;

            if (newValue == TileOrientation.FaceDown)
                ShowStats.Begin();
            else
                ShowLatestRoll.Begin();
        }

        public string UnplayedResourceCount(ObservableCollection<Entitlement> unspent, string name)
        {
            var entitlement = (Entitlement)Enum.Parse(typeof(Entitlement), name);
            var count = 0;
            foreach (var ent in unspent)
            {
                if (ent == entitlement) count++;
            }

            return count.ToString();
        }

        public PublicDataCtrl()
        {
            this.InitializeComponent();
        }

        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public TileOrientation RollOrientation
        {
            get => (TileOrientation)GetValue(RollOrientationProperty);
            set => SetValue(RollOrientationProperty, value);
        }

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PublicDataCtrl), new PropertyMetadata(null, PlayerChanged));
        public static readonly DependencyProperty RollOrientationProperty = DependencyProperty.Register("RollOrientation", typeof(TileOrientation), typeof(PublicDataCtrl), new PropertyMetadata(TileOrientation.FaceDown, RollOrientationChanged));
    }
}
