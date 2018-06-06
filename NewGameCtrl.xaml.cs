using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public enum DialogResult { Ok, Cancel };
    public sealed partial class NewGameDialog : UserControl
    {
        TaskCompletionSource<object> _tcs = null;
        DialogResult _result = DialogResult.Cancel;
        ObservableCollection<PlayerCtrl> _availablePlayers = new ObservableCollection<PlayerCtrl>();
        ObservableCollection<PlayerCtrl> _playingPlayers = new ObservableCollection<PlayerCtrl>();

        public NewGameDialog()
        {

            this.InitializeComponent();
            this.DataContext = this;
            _lvAvailablePlayers.ItemsSource = _availablePlayers;
            _lvPlaying.ItemsSource = _playingPlayers;



        }

        public ObservableCollection<PlayerCtrl> AvailablePlayers
        {
            get
            {
                return _availablePlayers;
            }
        }

        public ObservableCollection<PlayerCtrl> PlayingPlayers
        {
            get
            {
                return _playingPlayers;
            }
        }

        private void Reset()
        {
            _playingPlayers.Clear();
            _availablePlayers.Clear();

            _availablePlayers.Add(new PlayerCtrl(Player.Joe));
            _availablePlayers.Add(new PlayerCtrl(Player.Dodgy));
            _availablePlayers.Add(new PlayerCtrl(Player.Doug));
            _availablePlayers.Add(new PlayerCtrl(Player.Craig));
            _availablePlayers.Add(new PlayerCtrl(Player.Chris));
            _availablePlayers.Add(new PlayerCtrl(Player.Cort));
            _availablePlayers.Add(new PlayerCtrl(Player.Guest));
            _availablePlayers.Add(new PlayerCtrl(Player.Robert));
            _availablePlayers.Add(new PlayerCtrl(Player.John));


        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            _result = DialogResult.Ok;
            _tcs.SetResult(null);
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            _result = DialogResult.Cancel;
            _tcs.SetResult(null);
        }

        private void OnAddPlayer(object sender, RoutedEventArgs e)
        {
            var selected = _lvAvailablePlayers.SelectedItems.Cast<PlayerCtrl>().ToArray();

            foreach (PlayerCtrl p in selected)
            {
                _availablePlayers.Remove(p);
                _playingPlayers.Add(p);
            }

            UpdateAcceptButton();


        }

        private void UpdateAcceptButton()
        {
            if (_playingPlayers.Count >= 3)
                _btnAccept.IsEnabled = true;
            else
                _btnAccept.IsEnabled = false;
        }

        public ObservableCollection<PlayerCtrl> Players
        {
            get
            {
                return _playingPlayers;
            }
        }

        private void OnRemovePlayer(object sender, RoutedEventArgs e)
        {
            var selected = _lvPlaying.SelectedItems.Cast<PlayerCtrl>().ToArray();

            foreach (PlayerCtrl p in selected)
            {
                _playingPlayers.Remove(p);
                _availablePlayers.Add(p);
            }

            UpdateAcceptButton();

        }
        public async Task<DialogResult> ShowAndWait()
        {

            _tcs = new TaskCompletionSource<object>();
            Reset();
            await _tcs.Task;
            return _result;
        }

        private void OnContentChanged(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            int n = 1;
            foreach (PlayerCtrl p in _playingPlayers)
            {
                p.Index = n++;
            }


        }
    }
}
