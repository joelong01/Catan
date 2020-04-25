using Catan.Proxy;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PublicDataCtrl : UserControl
    {
        private ObservableCollection<DevCardType> PlayedDevCards { get; set; } = new ObservableCollection<DevCardType>();
        public static readonly DependencyProperty PlayerDataProperty = DependencyProperty.Register("PlayerData", typeof(PlayerModel), typeof(PublicDataCtrl), new PropertyMetadata(new PlayerModel(), PlayerChanged));
        public PlayerModel PlayerData
        {
            get => (PlayerModel)GetValue(PlayerDataProperty);
            set => SetValue(PlayerDataProperty, value);
        }
        private static void PlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PublicDataCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetPlayer(depPropValue);
        }
        private void SetPlayer(PlayerModel value)
        {
            PlayedDevCards.Clear();
            PlayedDevCards.AddRange(value.GameData.PlayerResources.PlayedDevCards);
            
            this.PlayedDevCards.Add(DevCardType.Knight);
            this.PlayedDevCards.Add(DevCardType.YearOfPlenty);
            this.PlayedDevCards.Add(DevCardType.Knight);
            this.PlayedDevCards.Add(DevCardType.Monopoly);
        }


        public PublicDataCtrl()
        {
            this.InitializeComponent();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is TextBox tb))
            {
                return;
            }

            tb.SelectAll();
        }


        private async void Picture_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            ellipse.IsTapEnabled = false;
            try
            {

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
