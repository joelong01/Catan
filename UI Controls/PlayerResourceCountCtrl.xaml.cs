using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PlayerResourceCountCtrl : UserControl
    {

        /// <summary>
        ///     this has the *global* resource count
        /// </summary>
        public PlayerResourceModel GlobalResourceCount { get; } = new PlayerResourceModel(null);
        public ILog Log { get; set; } = null;

        public PlayerResourceCountCtrl()
        {
            this.InitializeComponent();
            GlobalResourceCount.TurnReset();

        }



        public static readonly DependencyProperty MainPageProperty = DependencyProperty.Register("MainPage", typeof(MainPage), typeof(PlayerResourceCountCtrl), new PropertyMetadata(null));
        public MainPage MainPage
        {
            get => (MainPage)GetValue(MainPageProperty);
            set => SetValue(MainPageProperty, value);
        }


        /// <summary>
        ///    we subscribe to changes made for each player so we can update for the global
        ///    
        ///    in the global count we can't use newVal - oldVal to get the change because in TurnReset
        ///    newVal is 0.  So instead we go to the object to see what the value is.
        /// </summary>
        private void OnResourceUpdate(PlayerModel player, ResourceType resource, int currentValue, int newVal)
        {


            GlobalResourceCount.AddResourceCount(resource, newVal - currentValue);
            //
            //  logged by PlayerGameModel.OnPlayerResourceUpdate
        }

        public ObservableCollection<PlayerModel> TestPlayers { get; set; } = new ObservableCollection<PlayerModel>();

        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerModel>), typeof(PlayerResourceCountCtrl), new PropertyMetadata(new ObservableCollection<PlayerModel>(), PlayingPlayersChanged));
        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get => (ObservableCollection<PlayerModel>)GetValue(PlayingPlayersProperty);
            set => SetValue(PlayingPlayersProperty, value);
        }
        private static void PlayingPlayersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerResourceCountCtrl depPropClass = d as PlayerResourceCountCtrl;
            ObservableCollection<PlayerModel> depPropValue = (ObservableCollection<PlayerModel>)e.NewValue;
            depPropClass?.SetPlayingPlayers(depPropValue);
        }
        private void SetPlayingPlayers(ObservableCollection<PlayerModel> newList)
        {
           

            GlobalResourceCount.GameReset();

            ListBox_PlayerResourceCountList.ItemsSource = null;
            ListBox_PlayerResourceCountList.ItemsSource = newList;
            newList.CollectionChanged += PlayersChanged; // to track add/remove/clear players
            //
            //  this happens when somebody sets the PlayingPlayers to a new List
            foreach (var player in newList)
            {
                player.GameData.PlayerTurnResourceCount.OnPlayerResourceUpdate += OnResourceUpdate;
            }

            TestPlayers = newList;
        }

        private void PlayersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (PlayerModel player in e.NewItems)
                    {
                        player.GameData.PlayerTurnResourceCount.OnPlayerResourceUpdate += OnResourceUpdate;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (PlayerModel removedPlayer in e.OldItems)
                    {
                        removedPlayer.GameData.PlayerTurnResourceCount.OnPlayerResourceUpdate -= OnResourceUpdate;
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (e.OldItems == null) break;
                    foreach (PlayerModel removedPlayer in e.OldItems)
                    {
                        removedPlayer.GameData.PlayerTurnResourceCount.OnPlayerResourceUpdate -= OnResourceUpdate;
                    }
                    break;
                default:
                    break;
            }
        }

        
    }
}
