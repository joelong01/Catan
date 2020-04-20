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
        public string ErrorMessage { get; set; }
        private CatanProxy Proxy { get; } = new CatanProxy();
        private PlayerModel CurrentPlayer { get; set; } = null;
        private List<PlayerModel> AllPlayers { get; set; } = null;


        public ObservableCollection<PlayerModel> Players = new ObservableCollection<PlayerModel>();
        public ObservableCollection<PlayerModel> PlayersInGame = new ObservableCollection<PlayerModel>();
        public ObservableCollection<string> Games = new ObservableCollection<string>();

        public static readonly DependencyProperty NewGameNameProperty = DependencyProperty.Register("NewGameName", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata(""));
        public static readonly DependencyProperty SelectedGameProperty = DependencyProperty.Register("SelectedGame", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata("", SelectedGameChanged));
        public static readonly DependencyProperty HostNameProperty = DependencyProperty.Register("HostName", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata("", HostNameChanged));
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
        private void SetSelectedGame(string value)
        {
            this.TraceMessage($"Selected Game: {value}");
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
            Games.AddRange(games);
            if (games.Count > 0)
            {
                SelectedGame = games[0];
            }

        }

        private void OnCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

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
                using (var proxy = new CatanProxy() { HostName = HostName })
                {

                    List<string> games = await proxy.GetGames();
                    Games.Clear();
                    PlayersInGame.Clear();
                    Games.AddRange(games);
                }
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
                using (var proxy = new CatanProxy() { HostName = HostName })
                {
                    List<string> games = await proxy.CreateGame(NewGameName, new GameInfo());
                    Games.Clear();
                    PlayersInGame.Clear();
                    Games.AddRange(games);
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }

        }

        private async void OnJoin(object sender, RoutedEventArgs re)
        {
            ErrorMessage = "";
            try
            {
                using (var proxy = new CatanProxy() { HostName = HostName })
                {
                    var resources = await proxy.JoinGame(SelectedGame, CurrentPlayer.PlayerName);
                    if (resources != null)
                    {
                        await GetPlayersInGame();
                        return;

                    }
                    if (proxy.LastError != null)
                    {
                        ErrorMessage = proxy.LastError.Description;
                    }
                    else
                    {
                        ErrorMessage = "unexpected service error.  No Error message recieved.  Likely failed before getting to the service.";
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
            SelectedGame = e.AddedItems[0] as string;
            await GetPlayersInGame();
        }

        private async Task GetPlayersInGame()
        {
            ErrorMessage = "";
            if (String.IsNullOrEmpty(SelectedGame)) return;
            try
            {

                using (var proxy = new CatanProxy() { HostName = this.HostName })
                {

                    List<string> users = await proxy.GetUsers(SelectedGame);
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
                this.Hide();
                var answer = await StaticHelpers.AskUserYesNoQuestion($"Are you sure want to delete the game name {SelectedGame}?", "Yes", "No");
                if (!answer) return;

                using (var proxy = new CatanProxy() { HostName = this.HostName })
                {

                    var result = await proxy.DeleteGame(SelectedGame);
                    if (result == null)
                    {
                        ErrorMessage = proxy.LastError?.Description;
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
    }
}
