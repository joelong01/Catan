using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    public class GameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Expansion { get; set; }
        public DataTemplate Regular { get; set; }
        public ObservableCollection<PlayerModel> SourceCollection { get; set; }  

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (SourceCollection.Count >=5)
            {
                return Expansion;
            }
            else
            {
                return Regular;
            }
        }
    }

    public sealed partial class PlayersTrackerCtrl : UserControl
    {
        /// <summary>
        ///     this has the *global* resource count
        /// </summary>
        ///

        private static void MainPageModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayersTrackerCtrl;
            var depPropValue = (MainPageModel)e.NewValue;
            depPropClass?.SetMainPageModel(depPropValue);
        }

        private static void PlayingPlayersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayersTrackerCtrl;
            var depPropValue = (ObservableCollection<PlayerModel>)e.NewValue;
            depPropClass?.SetPlayingPlayers(depPropValue);
        }

        private void SetMainPageModel(MainPageModel mpm)
        {
        }

        private void SetPlayingPlayers(ObservableCollection<PlayerModel> value)
        {
        }

        public TradeResources GameResources
        {
            get => ( TradeResources )GetValue(GameResourcesProperty);
            set => SetValue(GameResourcesProperty, value);
        }

        public GameState GameState
        {
            get => ( GameState )GetValue(GameStateProperty);
            set => SetValue(GameStateProperty, value);
        }

        public MainPage MainPage
        {
            get => ( MainPage )GetValue(MainPageProperty);
            set => SetValue(MainPageProperty, value);
        }

        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }

        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get => ( ObservableCollection<PlayerModel> )GetValue(PlayingPlayersProperty);
            set => SetValue(PlayingPlayersProperty, value);
        }

        public static readonly DependencyProperty GameResourcesProperty = DependencyProperty.Register("GameResources", typeof(TradeResources), typeof(PlayersTrackerCtrl), new PropertyMetadata(new TradeResources()));

        public static readonly DependencyProperty GameStateProperty = DependencyProperty.Register("GameState", typeof(GameState), typeof(PlayersTrackerCtrl), new PropertyMetadata(GameState.WaitingForNewGame));

        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(PlayersTrackerCtrl), new PropertyMetadata(null, MainPageModelChanged));

        public static readonly DependencyProperty MainPageProperty = DependencyProperty.Register("MainPage", typeof(MainPage), typeof(PlayersTrackerCtrl), new PropertyMetadata(null));

        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerModel>), typeof(PlayersTrackerCtrl), new PropertyMetadata(null, PlayingPlayersChanged));

        public PlayersTrackerCtrl()
        {
            this.InitializeComponent();
        }

        public double ControlHeight
        {
            get
            {
                if (MainPageModel.PlayingPlayers.Count < 5)
                    return 225.0;

                return 175.0;
            }
        }

    }
}
