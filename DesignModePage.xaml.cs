using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{

  

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DesignModePage : Page
    {

        

        public DesignModePage()
        {
            this.InitializeComponent();
        }

        public GameViewControl GameView
        {
            get
            {
                return _gameView;
            }
            set
            {

            }
        }

        //public ObservableCollection<TileGroupItem> TileGroups
        //{
        //    get
        //    {
        //        return _gameView.TileGroups;
        //    }
        //}

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //if (e.Parameter!= null)
            //{
            //    _gameView = (GameViewControl)e.Parameter;
            //    return;
            //}

            if (e.NavigationMode == NavigationMode.New)
            {
                _progress.IsActive = true;
                _progress.Visibility = Visibility.Visible;
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                await _gameView.LoadAllGames();
                _gameView.SetDesignMode(true);
                _progress.IsActive = false;
                _progress.Visibility = Visibility.Collapsed;

            }




        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }

        private async void OnSaveGame(object sender, RoutedEventArgs e)
        {
            StorageFile file = await _gameView.Save();
        }

        private async void OnDelete(object sender, RoutedEventArgs e)
        {
            bool ret = await StaticHelpers.AskUserYesNoQuestion($"Delete Game \"{_gameView.CurrentGame}\"?", "Yes", "No");
            if (ret)
            {
                _gameView.DeleteCurrentGame(_cmbSavedGames.SelectedItem.ToString());
            }

        }
        private void CommandLine_KeyUp(object sender, KeyRoutedEventArgs e)
        {

        }

        private void CreateBoard(object sender, RoutedEventArgs e)
        {
            _gameView.CreateBoard();




        }

        private void OnExitDesignMode(object sender, RoutedEventArgs e)
        {

            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }


        }

        private void UseClassic_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void HarborLocationOpened(object sender, object e)
        {
            ComboBox box = sender as ComboBox;
            TileCtrl SelectedTile = _gameView.SelectedTile;
            foreach (CheckBox item in box.Items)
            {
                HarborLocation location = (HarborLocation)item.Tag;
                item.IsChecked = SelectedTile?.IsHarborVisible(location);
            }
            _gameView.HarborLocationsAsText = SelectedTile.HarborLocationsAsShortText;

        }



    }

    public class TileGroupItem
    {
        public int TileStart { get; set; }
        public int TileEnd { get; set; }
        public bool? Randomize { get; set; }

        public override string ToString()
        {
            return String.Format($"{TileStart}-{TileEnd}.{Randomize}");
        }

        public string Serialize()
        {
            return this.ToString();
        }
        public bool Deserialize(string s)
        {
            try
            {
                char[] seps = new char[] { '-', '.' };
                string[] tokens = s.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Count() != 3) return false;

                TileStart = Int32.Parse(tokens[0]);
                TileEnd = Int32.Parse(tokens[1]);
                Randomize = bool.Parse(tokens[2]);

            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
