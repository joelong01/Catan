using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PlayerResourceCountCtrl : UserControl
    {

        public PlayerResourceData GameResourceData { get; } = new PlayerResourceData(null);

        public PlayerResourceCountCtrl()
        {
            this.InitializeComponent();
            GameResourceData.Reset();
        }
        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerData>), typeof(PlayerResourceCountCtrl), new PropertyMetadata(new ObservableCollection<PlayerData>(), PlayingPlayersChanged));
        public ObservableCollection<PlayerData> PlayingPlayers
        {
            get => (ObservableCollection<PlayerData>)GetValue(PlayingPlayersProperty);
            set => SetValue(PlayingPlayersProperty, value);
        }
        private static void PlayingPlayersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayerResourceCountCtrl;
            var depPropValue = (ObservableCollection<PlayerData>)e.NewValue;
            depPropClass?.SetPlayingPlayers(depPropValue);
        }
        private void SetPlayingPlayers(ObservableCollection<PlayerData> newList)
        {
            ListBox_PlayerResourceCountList.ItemsSource = null;
            ListBox_PlayerResourceCountList.ItemsSource = newList;
            
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is TextBox tb)) return;

            tb.SelectAll();
        }

      
        private async void Picture_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            ellipse.IsTapEnabled = false;
            try
            {

                PlayerData player = ((Ellipse)sender).Tag as PlayerData;

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
