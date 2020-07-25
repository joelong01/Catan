using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class ServiceGameDlg : ContentDialog
    {


        #region Delegates + Fields + Events + Enums

        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata(""));
        public static readonly DependencyProperty HostNameProperty = DependencyProperty.Register("HostName", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata("", HostNameChanged));
        public static readonly DependencyProperty NewGameNameProperty = DependencyProperty.Register("NewGameName", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata(""));
        public static readonly DependencyProperty SelectedGameProperty = DependencyProperty.Register("SelectedGame", typeof(GameInfo), typeof(ServiceGameDlg), new PropertyMetadata(null, SelectedGameChanged));
        public ObservableCollection<GameInfo> Games = new ObservableCollection<GameInfo>();
        public ObservableCollection<PlayerModel> Players = new ObservableCollection<PlayerModel>();
        public ObservableCollection<PlayerModel> PlayersInGame = new ObservableCollection<PlayerModel>();
        private TaskCompletionSource<bool> _tcs = null;

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        public CatanGameData GameInfo { get; set; }
        public string HostName
        {
            get => (string)GetValue(HostNameProperty);
            set => SetValue(HostNameProperty, value);
        }

        public bool IsCanceled { get; private set; } = false;
        public bool JoinedExistingGame { get; private set; } = false;
        public string NewGameName
        {
            get => (string)GetValue(NewGameNameProperty);
            set => SetValue(NewGameNameProperty, value);
        }

        public GameInfo SelectedGame
        {
            get => (GameInfo)GetValue(SelectedGameProperty);
            set => SetValue(SelectedGameProperty, value);
        }

        private List<PlayerModel> AllPlayers { get; set; } = null;
        private PlayerModel CurrentPlayer { get; set; } = null;
        private ICatanService Proxy { get; }

        #endregion Properties

        #region Constructors + Destructors

        public ServiceGameDlg(PlayerModel currentPlayer, List<PlayerModel> players, List<GameInfo> games, ICatanService catanService)
        {
            this.InitializeComponent();
            CurrentPlayer = currentPlayer;
            AllPlayers = players;
            Proxy = catanService;
            Proxy.OnGameCreated += Proxy_OnGameCreated;
            Proxy.OnGameDeleted += Proxy_OnGameDeleted;
            Proxy.OnGameJoined += Proxy_OnGameJoined;
            Proxy.OnGameLeft += Proxy_OnGameLeft;
            if (games != null && games.Count > 0) Games.AddRange(games);
            if (Games.Count > 0)
            {
                SelectedGame = Games[0];
            }
        }

        #endregion Constructors + Destructors

        #region Methods

        private static void HostNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ServiceGameDlg;
            var depPropValue = (string)e.NewValue;
            depPropClass?.SetHostName(depPropValue);
        }

        private static void SelectedGameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ServiceGameDlg;
            var depPropValue = (GameInfo)e.NewValue;
            depPropClass?.SetSelectedGame(depPropValue);
        }

        private async Task<bool> AskUserQuestion(string question)
        {
            _tcs = new TaskCompletionSource<bool>();
            this.ErrorMessage = question;
            bool ret = await _tcs.Task;
            return ret;
        }

        private async Task GetGames()
        {
            ErrorMessage = "";
            try
            {
                List<GameInfo> games = await Proxy.GetAllGames();
                if (games == null)
                {
                    return;
                }
                Games.Clear();
                PlayersInGame.Clear();
                Games.AddRange(games);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }

        private async Task GetPlayersInGame()
        {
            ErrorMessage = "";

            try
            {
                List<string> players = await Proxy.GetAllPlayerNames(SelectedGame.Id);
                if (players == null)
                {

                    return;
                }
                PlayersInGame.Clear();
                foreach (var p in players)
                {
                    foreach (var pm in AllPlayers)
                    {
                        if (pm.PlayerName.Trim() == p)
                        {
                            PlayersInGame.Add(pm);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }

        private async void List_GameChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            SelectedGame = e.AddedItems[0] as GameInfo;
            await GetPlayersInGame();
        }

        private void OnCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            IsCanceled = true;
            UnscribeFromGameEvents();


        }
        private void UnscribeFromGameEvents()
        {
            Proxy.OnGameCreated -= Proxy_OnGameCreated;
            Proxy.OnGameDeleted -= Proxy_OnGameDeleted;
            Proxy.OnGameJoined -= Proxy_OnGameJoined;
            Proxy.OnGameLeft -= Proxy_OnGameLeft;
        }

        private void OnCancelError(object sender, RoutedEventArgs e)
        {
            ErrorMessage = "";
            if (_tcs == null) return;
            if (!_tcs.Task.IsCompleted) _tcs.SetResult(false);
        }

        private async void OnDelete(object sender, RoutedEventArgs re)
        {
            ErrorMessage = "";
            if (SelectedGame == null) return;

            try
            {
                bool ret = await AskUserQuestion($"Are you sure want to delete the game named \"{SelectedGame.Name}\"?");
                if (ret)
                {
                    await Proxy.DeleteGame(SelectedGame, MainPage.Current.TheHuman.PlayerName);

                    PlayersInGame.Clear();
                    Games.Remove(SelectedGame);

                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }

        private async void OnDeleteAll(object sender, RoutedEventArgs e)
        {
            bool ret = await AskUserQuestion($"Are you sure want to delete all the games?");
            if (ret)
            {
                Games.ForEach(async (game) =>
                {
                    await Proxy.DeleteGame(game, MainPage.Current.TheHuman.PlayerName);

                });

                PlayersInGame.Clear();
                Games.Clear();
            }
        }

        private async void OnJoin(object sender, RoutedEventArgs re)
        {
            ErrorMessage = "";
            try
            {
                foreach (var player in PlayersInGame)
                {
                    if (player.PlayerName == CurrentPlayer.PlayerName)
                    {
                        JoinedExistingGame = true;
                        this.Hide();
                        return;
                    }
                }
                this.Hide();
                var gameInfo = await Proxy.JoinGame(SelectedGame, CurrentPlayer.PlayerName);
                if (gameInfo != null)
                {
                    SelectedGame = gameInfo;
                    
                }

                ErrorMessage = "unexpected service error.  No Error message recieved.  Likely failed before getting to the service.";
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }

        private async void OnNew(object sender, RoutedEventArgs re)
        {
            ErrorMessage = "";
            try
            {
                GameInfo gameInfo = new GameInfo() { Id = Guid.NewGuid(), Name = NewGameName, Creator = CurrentPlayer.PlayerName };
                //
                //  
                // await Proxy.CreateGame(gameInfo);

                await CreateGameModel.CreateGame(MainPage.Current, gameInfo);

                 // added to the UI in the event handler
                
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                this.TraceMessage($"Exception: {ErrorMessage}");
            }
        }

        private void OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            IsCanceled = false;
            UnscribeFromGameEvents();
        }

        private void OnOkError(object sender, RoutedEventArgs e)
        {
            ErrorMessage = "";
            if (_tcs == null) return;
            if (!_tcs.Task.IsCompleted) _tcs.SetResult(true);
        }

        private async void OnRefresh(object sender, RoutedEventArgs e)
        {
            await GetGames();
        }

        private void Proxy_OnGameCreated(GameInfo gameInfo, string playerName)
        {
          //  this.TraceMessage($"{playerName} created {gameInfo.Name}");

            Games.Add(gameInfo);
            SelectedGame = gameInfo;

        }

        private void Proxy_OnGameDeleted(GameInfo gameInfo, string by)
        {
            this.TraceMessage($"{by} deleted {gameInfo}");
        }

        private void Proxy_OnGameJoined(GameInfo gameInfo, string playerName)
        {
            this.TraceMessage($"{playerName} joined {gameInfo.Name}");
        }

        private void Proxy_OnGameLeft(GameInfo gameInfo, string playerName)
        {
            this.TraceMessage($"{playerName} left {gameInfo.Name}");
        }

        private void SetHostName(string value)
        {

        }

        private void SetSelectedGame(GameInfo value)
        {
            // this.TraceMessage($"New Game: {value?.Description}");
        }

        #endregion Methods



        #region Constructors

        #endregion Constructors
    }

    public class ServiceGameDlgModel : INotifyPropertyChanged
    {


        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Delegates + Fields + Events + Enums

        #region Methods

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}
