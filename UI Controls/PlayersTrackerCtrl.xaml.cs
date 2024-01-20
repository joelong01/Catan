using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    public class GameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Expansion { get; set; }
        public DataTemplate Expansion_CitesAndKnights { get; set; }
        public DataTemplate Regular { get; set; }
        public DataTemplate Regular_CitiesAndKnights { get; set; }
        public bool CitiesAndKnights { get; set; }
        public ObservableCollection<PlayerModel> SourceCollection { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (SourceCollection.Count >= 5)
            {
                if (CitiesAndKnights)
                {
                    return Expansion_CitesAndKnights;
                }
                return Expansion;
            }
            else
            {
                if (CitiesAndKnights)
                    return Regular_CitiesAndKnights;

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

        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerModel>), typeof(PlayersTrackerCtrl), new PropertyMetadata(null, PlayingPlayersChanged));

        public PlayersTrackerCtrl()
        {
            this.InitializeComponent();
        }
        public double ControlWidth
        {
            get
            {
                if (ListBox_PlayerResourceCountList.Items.Count == 0) return 600;
                var item = ListBox_PlayerResourceCountList.Items[0]; 
                var listViewItem = ListBox_PlayerResourceCountList.ContainerFromItem(item) as ListViewItem;

                if (listViewItem != null)
                {
                    double width = listViewItem.ActualWidth;
                    return width;
                }

                return 600;

            }
        }
        public double ControlHeight(ObservableCollection<PlayerModel> players)
        {
            var max = 263;
            var totalAvailableHeight = ListBox_PlayerResourceCountList.ActualHeight;
            var itemHeight = totalAvailableHeight / players.Count;
            if (double.IsNaN(itemHeight)) return max;
            if (itemHeight > 0 && itemHeight < max )
                return itemHeight;

            return max;
 

        }

    }
}
