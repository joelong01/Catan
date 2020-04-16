using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
        private CatanProxy Proxy { get; } = new CatanProxy();
        #region properties
        public string SelectedGame { get; set; } = "";
        public ObservableCollection<PlayerModel> Players = new ObservableCollection<PlayerModel>();
        public ObservableCollection<string> PlayersInGame = new ObservableCollection<string>();
        public ObservableCollection<string> Games = new ObservableCollection<string>();
        public static readonly DependencyProperty HostNameProperty = DependencyProperty.Register("HostName", typeof(ObservableCollection<PlayerModel>), typeof(ServiceGameDlg), new PropertyMetadata("https://localhost:5000"));
        public static readonly DependencyProperty SelectedPlayerProperty = DependencyProperty.Register("SelectedPlayer", typeof(PlayerModel), typeof(ServiceGameDlg), new PropertyMetadata(null));
        public static readonly DependencyProperty PlayerSelectedIndexProperty = DependencyProperty.Register("PlayerSelectedIndex", typeof(int), typeof(ServiceGameDlg), new PropertyMetadata(0));
        public static readonly DependencyProperty NewPlayerProperty = DependencyProperty.Register("NewPlayer", typeof(string), typeof(ServiceGameDlgModel), new PropertyMetadata(""));
        public static readonly DependencyProperty NewGameNameProperty = DependencyProperty.Register("NewGameName", typeof(string), typeof(ServiceGameDlgModel), new PropertyMetadata(""));
        public string NewGameName
        {
            get => (string)GetValue(NewGameNameProperty);
            set => SetValue(NewGameNameProperty, value);
        }
        public string NewPlayer
        {
            get => (string)GetValue(NewPlayerProperty);
            set => SetValue(NewPlayerProperty, value);
        }
        public int PlayerSelectedIndex
        {
            get => (int)GetValue(PlayerSelectedIndexProperty);
            set => SetValue(PlayerSelectedIndexProperty, value);
        }

        public PlayerModel SelectedPlayer
        {
            get => (PlayerModel)GetValue(SelectedPlayerProperty);
            set => SetValue(SelectedPlayerProperty, value);
        }



        public string HostName
        {
            get => (string)GetValue(HostNameProperty);
            set => SetValue(HostNameProperty, value);
        }
        #endregion
        public ServiceGameDlg()
        {
            this.InitializeComponent();
        }

        public ServiceGameDlg(SavedState savedAppState)
        {
            this.InitializeComponent();
            Players.AddRange(savedAppState.Players); // extension function
            HostName = savedAppState.ServiceState.HostName;
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
            string msg = "";
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
                msg = e.Message;
            }

            finally
            {
                if (!String.IsNullOrEmpty(msg))
                {
                    await this.ShowAsync();
                }
            }
            if (!string.IsNullOrEmpty(msg))
            {
                this.Hide();
                await StaticHelpers.ShowErrorText(msg);
            }
        }

        private async void OnNew(object sender, RoutedEventArgs re)
        {
            string msg = "";
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
                msg = e.Message;
            }
            finally
            {
                if (!String.IsNullOrEmpty(msg))
                {
                    await this.ShowAsync();
                }
            }
            if (!string.IsNullOrEmpty(msg))
            {
                this.Hide();
                await StaticHelpers.ShowErrorText(msg);
            }
        }

        private async void OnJoin(object sender, RoutedEventArgs re)
        {
            string msg = "";
            if (PlayerSelectedIndex == -1) return;
            try
            {
                using (var proxy = new CatanProxy() { HostName = HostName })
                {
                    var resources = await proxy.JoinGame(SelectedGame, Players[PlayerSelectedIndex].PlayerName);
                    if (resources != null)
                    {
                        await GetPlayersInGame();
                        return;
                                               
                    }

                    msg = proxy.LastError.Description;

                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            finally
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    this.Hide();
                    await StaticHelpers.ShowErrorText(msg);
                    await this.ShowAsync();
                }
            }
            
        }

        private void OnPlayerChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null) return;
            if (e.AddedItems.Count == 0) return;
            NewPlayer = ((PlayerModel)e.AddedItems[0]).PlayerName;
        }

        private async void List_GameChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            SelectedGame = e.AddedItems[0] as string;
            await GetPlayersInGame();
        }

        private async Task GetPlayersInGame()
        {
            string msg = "";
            if (String.IsNullOrEmpty(SelectedGame)) return;
            try
            {
                
                using (var proxy = new CatanProxy() { HostName = this.HostName })
                {

                    List<string> users = await proxy.GetUsers(SelectedGame);
                    PlayersInGame.Clear();
                    PlayersInGame.AddRange(users);
                }
            }
            catch (Exception e)
            {

                msg = e.Message;
            }
            finally
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    this.Hide();
                    await StaticHelpers.ShowErrorText(msg);
                    await this.ShowAsync();
                }
            }
        }

        private async void OnDelete(object sender, RoutedEventArgs re)
        {
            string msg = "";
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
                        msg = proxy.LastError?.Description;
                    }
                    PlayersInGame.Clear();
                    await GetGames();
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            finally
            {
                if (!string.IsNullOrEmpty(msg))
                {                    
                    await StaticHelpers.ShowErrorText(msg);                   
                }
                await this.ShowAsync();
            }
        }
    }
}
