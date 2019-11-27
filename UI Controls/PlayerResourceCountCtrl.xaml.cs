using System.Collections.ObjectModel;
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
        public PlayerResourceData GameResourceData { get; } = new PlayerResourceData(null);
        public ILog Log { get; set; } = null;

        public PlayerResourceCountCtrl()
        {
            this.InitializeComponent();
            GameResourceData.TurnReset();
            GameResourceData.OnPlayerResourceUpdate += OnResourceUpdate;
        }

        public static readonly DependencyProperty MainPageProperty = DependencyProperty.Register("MainPage", typeof(MainPage), typeof(PlayerResourceCountCtrl), new PropertyMetadata(null));
        public MainPage MainPage
        {
            get => (MainPage)GetValue(MainPageProperty);
            set => SetValue(MainPageProperty, value);
        }

        
        /// <summary>
        ///     Unlike the logging in PlayerGameData, there is no PlayerData so that whole path isn't set up
        ///     here PlayerData is always null - we need to special case that in the UndoLogLine
        /// </summary>
        private void OnResourceUpdate(PlayerData player, ResourceType resource, int oldVal, int newVal)
        {
            System.Diagnostics.Debug.Assert(player == null, "Player shoudl be null in the global reource tracker");
            Log.PostLogEntry(player, GameState.Unknown,
                                                            CatanAction.AddResourceCount, false, LogType.Normal, newVal - oldVal,
                                                            new LogResourceCount(oldVal, newVal, resource));
        }

        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerData>), typeof(PlayerResourceCountCtrl), new PropertyMetadata(new ObservableCollection<PlayerData>(), PlayingPlayersChanged));
        public ObservableCollection<PlayerData> PlayingPlayers
        {
            get => (ObservableCollection<PlayerData>)GetValue(PlayingPlayersProperty);
            set => SetValue(PlayingPlayersProperty, value);
        }
        private static void PlayingPlayersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerResourceCountCtrl depPropClass = d as PlayerResourceCountCtrl;
            ObservableCollection<PlayerData> depPropValue = (ObservableCollection<PlayerData>)e.NewValue;
            depPropClass?.SetPlayingPlayers(depPropValue);
        }
        private void SetPlayingPlayers(ObservableCollection<PlayerData> newList)
        {
            ListBox_PlayerResourceCountList.ItemsSource = null;
            ListBox_PlayerResourceCountList.ItemsSource = newList;

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
