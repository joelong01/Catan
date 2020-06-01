using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PlayersTrackerCtrl : UserControl
    {
        /// <summary>
        ///     this has the *global* resource count
        /// </summary>
        /// 

        

        public static readonly DependencyProperty GameResourcesProperty = DependencyProperty.Register("GameResources", typeof(TradeResources), typeof(PlayersTrackerCtrl), new PropertyMetadata(new TradeResources()));
        public static readonly DependencyProperty GameStateProperty = DependencyProperty.Register("GameState", typeof(GameState), typeof(PlayersTrackerCtrl), new PropertyMetadata(GameState.WaitingForNewGame));
        public static readonly DependencyProperty MainPageProperty = DependencyProperty.Register("MainPage", typeof(MainPage), typeof(PlayersTrackerCtrl), new PropertyMetadata(null));
        
        public PlayersTrackerCtrl()
        {
            this.InitializeComponent();
            this.DataContext = PlayingPlayers;
        }

        public TradeResources GameResources
        {
            get => (TradeResources)GetValue(GameResourcesProperty);
            set => SetValue(GameResourcesProperty, value);
        }

        public GameState GameState
        {
            get => (GameState)GetValue(GameStateProperty);
            set => SetValue(GameStateProperty, value);
        }


        public MainPage MainPage
        {
            get => (MainPage)GetValue(MainPageProperty);
            set => SetValue(MainPageProperty, value);
        }
        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerModel>), typeof(PlayersTrackerCtrl), new PropertyMetadata(null, PlayingPlayersChanged));
        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get => (ObservableCollection<PlayerModel>)GetValue(PlayingPlayersProperty);
            set => SetValue(PlayingPlayersProperty, value);
        }
        private static void PlayingPlayersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayersTrackerCtrl;
            var depPropValue = (ObservableCollection<PlayerModel>)e.NewValue;
            depPropClass?.SetPlayingPlayers(depPropValue);
        }
        private void SetPlayingPlayers(ObservableCollection<PlayerModel> value)
        {

        }


    }
}