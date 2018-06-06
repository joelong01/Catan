using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class BaronPlacedDlg : ContentDialog
    {
        int _index = 1;

        public BaronPlacedDlg()
        {
            this.InitializeComponent();
        }

        public BaronPlacedDlg(ObservableCollection<CatanPlayer> players)
        {
            this.InitializeComponent();
            if (Application.Current is App)
                Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;

            ObservableCollection<CatanPlayer> copyPlayers = new ObservableCollection<CatanPlayer>();
            foreach (var p in players)
            {
                CatanPlayer newP = new CatanPlayer(p);                
                copyPlayers.Add(newP);
            }

            _playersView.Players = copyPlayers;
            _txtInput.Text = "1";
            _txtInput.Focus(FocusState.Programmatic);
            _txtInput.SelectAll();
        }
          public async Task LoadImages()
        {
            foreach (var player in _playersView.Players)
            {
                await player.LoadImage();
            }
        }
        private async void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            args.Handled = false;

            if (args.EventType != CoreAcceleratorKeyEventType.KeyUp)
                return;

            if (args.VirtualKey != VirtualKey.Enter)
                return;


            await OnEnter();
            _txtInput.Focus(FocusState.Programmatic);
            _txtInput.SelectAll();
            args.Handled = true;


        }

        private async Task OnEnter()
        {
            if (_txtInput.Text == "/")
            {
                PrepForDismiss();
                this.Hide();
                return;
            }

          
            if (Int32.TryParse(_txtInput.Text, out _index))
            {
                try
                {
                    await _playersView.AnimateToPlayerNumber(_index -1 );
                }
                catch
                {
                    
                }
                return;
            }
        }
        private void PrepForDismiss()
        {
            Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            _playersView.DetachPlayersFromGrid();
        }

        public ObservableCollection<CatanPlayer> Players
        {
            
            get
            {
                return _playersView.Players;
            }
            set
            {
                _playersView.Players = value;
                
            }
            
        }

        public int PlayerIndex
        {
            get
            {
                return _index;
            }
        }

        private void ContentDialog_OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void ContentDialog_OnCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private async void OnEnter(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await OnEnter();
            ((Button)sender).IsEnabled = true;
        }
    }
}
