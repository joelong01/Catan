using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
                
    }
}
