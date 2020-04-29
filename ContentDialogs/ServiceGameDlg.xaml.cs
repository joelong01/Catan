using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public class ServiceGameDlgModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public sealed partial class ServiceGameDlg : ContentDialog
    {
        #region properties

        private CatanProxy Proxy { get; } = MainPage.Current.MainPageModel.ServiceData.Proxy;
        private PlayerModel CurrentPlayer { get; set; } = null;
        private List<PlayerModel> AllPlayers { get; set; } = null;
        private TaskCompletionSource<bool> _tcs = null;
        public bool IsCanceled { get; private set; } = false;

        public ObservableCollection<PlayerModel> Players = new ObservableCollection<PlayerModel>();
        public ObservableCollection<PlayerModel> PlayersInGame = new ObservableCollection<PlayerModel>();
        public ObservableCollection<string> Games = new ObservableCollection<string>();
        public GameInfo GameInfo { get; set; }
        public static readonly DependencyProperty NewGameNameProperty = DependencyProperty.Register("NewGameName", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata(""));
        public static readonly DependencyProperty SelectedGameProperty = DependencyProperty.Register("SelectedGame", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata("", SelectedGameChanged));
        public static readonly DependencyProperty HostNameProperty = DependencyProperty.Register("HostName", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata("", HostNameChanged));
        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata(""));
        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }
        public string HostName
        {
            get => (string)GetValue(HostNameProperty);
            set => SetValue(HostNameProperty, value);
        }
        private static void HostNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ServiceGameDlg;
            var depPropValue = (string)e.NewValue;
            depPropClass?.SetHostName(depPropValue);
        }
        private void SetHostName(string value)
        {
            Proxy.HostName = value;
        }
        public string SelectedGame
        {
            get => (string)GetValue(SelectedGameProperty);
            set => SetValue(SelectedGameProperty, value);
        }
        private static void SelectedGameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ServiceGameDlg;
            var depPropValue = (string)e.NewValue;
            depPropClass?.SetSelectedGame(depPropValue);
        }
        private async void SetSelectedGame(string value)
        {
            if (String.IsNullOrEmpty(value)) return;

            GameInfo = await Proxy.GetGameInfo(value);
        }



        public string NewGameName
        {
            get => (string)GetValue(NewGameNameProperty);
            set => SetValue(NewGameNameProperty, value);
        }



        #endregion
        public ServiceGameDlg()
        {
            this.InitializeComponent();
        }

        public ServiceGameDlg(PlayerModel currentPlayer, List<PlayerModel> players, List<string> games)
        {
            this.InitializeComponent();
            CurrentPlayer = currentPlayer;
            AllPlayers = players;
            if (games != null) Games.AddRange(games);
            if (Games.Count > 0)
            {
                SelectedGame = Games[0];
            }

        }

        private void OnCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            IsCanceled = true;
        }

        private void OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            IsCanceled = false;
        }

        private async void OnRefresh(object sender, RoutedEventArgs e)
        {
            await GetGames();
        }

        private async Task GetGames()
        {
            ErrorMessage = "";
            try
            {
                List<string> games = await Proxy.GetGames();
                if (games == null)
                {
                    ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
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

        private async void OnNew(object sender, RoutedEventArgs re)
        {
            ErrorMessage = "";
            try
            {
                GameInfo = new GameInfo
                {
                    BoardSettings = MainPage.Current.GameContainer.GetRandomBoard()
                };

                List<string> games = await Proxy.CreateGame(NewGameName, GameInfo);
                if (games == null)
                {
                    ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
                    return;
                }
                if (games != null && games.Count > 0)
                {
                    Games.Clear();
                    PlayersInGame.Clear();
                    Games.AddRange(games);
                    SelectedGame = Games[Games.Count - 1];
                }


            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                this.TraceMessage($"Exception: {ErrorMessage}");
            }

        }

        private async void OnJoin(object sender, RoutedEventArgs re)
        {
            ErrorMessage = "";
            try
            {

                var gameLog = await Proxy.JoinGame(SelectedGame, CurrentPlayer.PlayerName);
                GameInfo = gameLog.GameInfo;
                if (GameInfo != null)
                {
                    PlayersInGame.Clear();
                    foreach (var p in gameLog.Players)
                    {
                        foreach (var pm in AllPlayers)
                        {
                            if (pm.PlayerName.ToLower().Trim() == p)
                            {
                                PlayersInGame.Add(pm);
                                break;
                            }
                        }

                    }
                    this.Hide();
                    return;

                }
                if (Proxy.LastError != null)
                {
                    ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
                }
                else
                {
                    ErrorMessage = "unexpected service error.  No Error message recieved.  Likely failed before getting to the service.";
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
            SelectedGame = e.AddedItems[0] as string;
            await GetPlayersInGame();
        }

        private async Task GetPlayersInGame()
        {
            ErrorMessage = "";
            if (String.IsNullOrEmpty(SelectedGame)) return;
            try
            {
                List<string> users = await Proxy.GetUsers(SelectedGame);
                if (users == null)
                {
                    ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
                    return;
                }
                PlayersInGame.Clear();
                foreach (var p in users)
                {
                    foreach (var pm in AllPlayers)
                    {
                        if (pm.PlayerName.ToLower().Trim() == p)
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

        private async void OnDelete(object sender, RoutedEventArgs re)
        {
            ErrorMessage = "";
            if (String.IsNullOrEmpty(SelectedGame)) return;
            try
            {

                bool ret = await AskUserQuestion($"Are you sure want to delete the game name {SelectedGame}?");
                if (ret)
                {
                    var result = await Proxy.DeleteGame(SelectedGame);
                    if (result == null)
                    {
                        ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
                        return;
                    }
                    PlayersInGame.Clear();
                    await GetGames();
                }

            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }

        private async Task<bool> AskUserQuestion(string question)
        {
            _tcs = new TaskCompletionSource<bool>();
            this.ErrorMessage = question;
            bool ret = await _tcs.Task;
            return ret;
        }

        private void OnCancelError(object sender, RoutedEventArgs e)
        {
            ErrorMessage = "";
            if (!_tcs.Task.IsCompleted) _tcs.SetResult(false);
        }

        private void OnOkError(object sender, RoutedEventArgs e)
        {
            ErrorMessage = "";
            if (_tcs == null) return;
            if (!_tcs.Task.IsCompleted) _tcs.SetResult(true);
        }
    }
}
