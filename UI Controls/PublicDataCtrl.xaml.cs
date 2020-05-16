using Catan.Proxy;
using System.Collections.ObjectModel;
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
        private ObservableCollection<DevCardType> PlayedDevCards { get; set; } = new ObservableCollection<DevCardType>();
        
        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PublicDataCtrl), new PropertyMetadata(new PlayerModel(), PlayerChanged));
        public static readonly DependencyProperty GameStateProperty = DependencyProperty.Register("GameState", typeof(GameState), typeof(PublicDataCtrl), new PropertyMetadata(GameState.WaitingForNewGame, GameStateChanged));
        public static readonly DependencyProperty RollOrientationProperty = DependencyProperty.Register("RollOrientation", typeof(TileOrientation), typeof(PublicDataCtrl), new PropertyMetadata(TileOrientation.FaceDown, RollOrientationChanged));
        public static readonly DependencyProperty GradientStopOneColorProperty = DependencyProperty.Register("GradientStopOneColor", typeof(Color), typeof(PublicDataCtrl), new PropertyMetadata(Colors.SlateBlue));
        public static readonly DependencyProperty GradientStopTwoColorProperty = DependencyProperty.Register("GradientStopTwoColor", typeof(Color), typeof(PublicDataCtrl), new PropertyMetadata(Colors.Black));
        public Color GradientStopTwoColor
        {
            get => (Color)GetValue(GradientStopTwoColorProperty);
            set => SetValue(GradientStopTwoColorProperty, value);
        }
        public Color GradientStopOneColor
        {
            get => (Color)GetValue(GradientStopOneColorProperty);
            set => SetValue(GradientStopOneColorProperty, value);
        }
        public TileOrientation RollOrientation
        {
            get => (TileOrientation)GetValue(RollOrientationProperty);
            set => SetValue(RollOrientationProperty, value);
        }
        private static void RollOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PublicDataCtrl;
            var depPropValue = (TileOrientation)e.NewValue;
            depPropClass?.SetRollOrientation(depPropValue);
        }
        private void SetRollOrientation(TileOrientation orientation)
        {
            if (Player == null) return;

            if (orientation == TileOrientation.FaceDown) 
                ShowStats.Begin();
            else
                ShowLatestRoll.Begin();
        }

        public GameState GameState
        {
            get => (GameState)GetValue(GameStateProperty);
            set => SetValue(GameStateProperty, value);
        }
        private static void GameStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PublicDataCtrl;
            var depPropValue = (GameState)e.NewValue;
            depPropClass?.SetGameState(depPropValue);
        }
        private void SetGameState(GameState state)
        {
            if (state == GameState.WaitingForRollForOrder)
            {
                ShowLatestRoll.Begin();
            }
            else
            {
                ShowStats.Begin();
            }
        }

        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }
        private static void PlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PublicDataCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetPlayer(depPropValue);
        }
       
        private void SetPlayer(PlayerModel value)
        {
            if (value == null) return;
            PlayedDevCards.Clear();
            PlayedDevCards.AddRange(value.GameData.PlayerResources.PlayedDevCards);            
            //
            //  for testing...
            //
            this.PlayedDevCards.Add(DevCardType.Knight);
            this.PlayedDevCards.Add(DevCardType.YearOfPlenty);
            this.PlayedDevCards.Add(DevCardType.Knight);
            this.PlayedDevCards.Add(DevCardType.Monopoly);
            
        }

       

        public PublicDataCtrl()
        {
            this.InitializeComponent();
            
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
                    await MainPage.Current.SetFirst(player); //manipulates the shared PlayingPlayers list, but also does logging and other book keeping.
                }
            }
            finally
            {
                ellipse.IsTapEnabled = true;
            }

        }
    }
}
