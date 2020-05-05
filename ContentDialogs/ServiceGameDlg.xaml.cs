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
        public ObservableCollection<SessionInfo> Sessions = new ObservableCollection<SessionInfo>();
        public GameInfo GameInfo { get; set; }
        public static readonly DependencyProperty NewSessionNameProperty = DependencyProperty.Register("NewSessionName", typeof(string), typeof(ServiceGameDlg), new PropertyMetadata(""));
        public static readonly DependencyProperty SelectedSessionProperty = DependencyProperty.Register("SelectedSession", typeof(SessionInfo), typeof(ServiceGameDlg), new PropertyMetadata(null, SelectedSessionChanged));
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
        public SessionInfo SelectedSession
        {
            get => (SessionInfo)GetValue(SelectedSessionProperty);
            set => SetValue(SelectedSessionProperty, value);
        }
        private static void SelectedSessionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ServiceGameDlg;
            var depPropValue = (SessionInfo)e.NewValue;
            depPropClass?.SetSelectedSession(depPropValue);
        }
        private void SetSelectedSession(SessionInfo value)
        {
            this.TraceMessage($"New Session: {value?.Description}");

        }



        public string NewSessionName
        {
            get => (string)GetValue(NewSessionNameProperty);
            set => SetValue(NewSessionNameProperty, value);
        }



        #endregion
        public ServiceGameDlg()
        {
            this.InitializeComponent();
        }

        public ServiceGameDlg(PlayerModel currentPlayer, List<PlayerModel> players, List<SessionInfo> sessions)
        {
            this.InitializeComponent();
            CurrentPlayer = currentPlayer;
            AllPlayers = players;
            if (sessions != null && sessions.Count > 0) Sessions.AddRange(sessions);
            if (Sessions.Count > 0)
            {
                SelectedSession = Sessions[0];
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
            await GetSessions();
        }

        private async Task GetSessions()
        {
            ErrorMessage = "";
            try
            {
                List<SessionInfo> sessions = await Proxy.GetSessions();
                if (sessions == null)
                {
                    ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
                    return;
                }
                Sessions.Clear();
                PlayersInGame.Clear();
                Sessions.AddRange(sessions);
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

                SessionInfo sessionInfo = new SessionInfo() { Id = Guid.NewGuid().ToString(), Description = NewSessionName, Creator = MainPage.Current.TheHuman.PlayerName };

                List<SessionInfo> sessions = await Proxy.CreateSession(sessionInfo);
                if (sessions == null)
                {
                    ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
                    return;
                }
                if (sessions != null && sessions.Count > 0)
                {
                    Sessions.Clear();
                    PlayersInGame.Clear();
                    Sessions.AddRange(sessions);
                    SelectedSession = Sessions[Sessions.Count - 1];
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

                var sessionInfo =  await Proxy.JoinSession(SelectedSession.Id, CurrentPlayer.PlayerName);
                if (sessionInfo != null)
                {
                    SelectedSession = sessionInfo;
                    this.Hide();
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



        private async void List_SessionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            SelectedSession = e.AddedItems[0] as SessionInfo;
            await GetPlayersInSession();
        }

        private async Task GetPlayersInSession()
        {
            ErrorMessage = "";
          
            try
            {
                List<string> players = await Proxy.GetPlayers(SelectedSession.Id);
                if (players == null)
                {
                    ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
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

        private async void OnDelete(object sender, RoutedEventArgs re)
        {
            ErrorMessage = "";
            if (SelectedSession == null) return;
            
            try
            {

                bool ret = await AskUserQuestion($"Are you sure want to delete the session named \"{SelectedSession.Description}\"?");
                if (ret)
                {
                    var sessions = await Proxy.DeleteSession(SelectedSession.Id);
                    if (sessions == null)
                    {
                        ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
                        return;
                    }
                    PlayersInGame.Clear();
                    Sessions.Clear();
                    Sessions.AddRange(sessions);
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
