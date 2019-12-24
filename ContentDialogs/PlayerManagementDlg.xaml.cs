using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class PlayerManagementDlg : ContentDialog
    {
        private ILog _log = null;

        public PlayerManagementDlg(ILog log)
        {
            this.InitializeComponent();
            _log = log;
        }




        public ObservableCollection<PlayerModel> PlayerDataList { get; } = new ObservableCollection<PlayerModel>();



        public PlayerManagementDlg(IList<PlayerModel> playerData, ILog log)
        {
            this.InitializeComponent();
            _log = log;
            PlayerDataList.AddRange(playerData);

        }



        private async void OnAddPlayer(object sender, RoutedEventArgs e)
        {
            PlayerModel pd = new PlayerModel(_log);
            await pd.LoadImage();
            PlayerDataList.Add(pd);
            _gvPlayers.ScrollIntoView(pd);
        }

        private void OnDeletePlayer(object sender, RoutedEventArgs e)
        {
            if (_selectedPlayer != null)
            {
                PlayerDataList.Remove(_selectedPlayer);
                _selectedPlayer = PlayerDataList.FirstOrDefault();
                _selectedPlayer.IsCurrentPlayer = true;
                _gvPlayers.ScrollIntoView(_selectedPlayer);
            }
        }


        private void OnPlayerColorChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnCurrentPlayerChanged(object sender, SelectionChangedEventArgs e)
        {

            if (e.AddedItems.Count == 0)
            {
                if (e.RemovedItems.Count > 0)
                {
                    //
                    //   unselected
                    _selectedPlayer.IsCurrentPlayer = false;
                    _selectedPlayer = null;

                }

                return;
            }

            PlayerModel player = e.AddedItems[0] as PlayerModel; // single select only
            if (_selectedPlayer != null)
            {
                _selectedPlayer.IsCurrentPlayer = false;
            }
            _selectedPlayer = player;
            _selectedPlayer.IsCurrentPlayer = true;
            this.TraceMessage($"Selected {_selectedPlayer}");


        }

        PlayerModel _selectedPlayer = null;

        private void OnSavePlayer(object sender, RoutedEventArgs e)
        {

        }

        private async void OnImageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            PlayerModel data = ((Grid)sender).Tag as PlayerModel;
            await LoadNewImage(data);

        }

        private async System.Threading.Tasks.Task LoadNewImage(PlayerModel player)
        {
            _gvPlayers.IsEnabled = false;
            _gvPlayers.SelectedItem = player;
            try
            {
                Windows.Storage.StorageFolder folder = await StaticHelpers.GetSaveFolder();
                FileOpenPicker picker = new FileOpenPicker()
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".gif");

                Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {

                    if (player != null)
                    {
                        Windows.Storage.StorageFile fileCopy = await player.CopyImage(file);
                        player.ImageFileName = fileCopy.Name;
                        await player.LoadImage();

                    }

                }
            }
            finally
            {
                _gvPlayers.IsEnabled = true;
            }
        }

        private async void OnSaveAndclose(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await MainPage.SavePlayers(PlayerDataList, "players.data");
            args.Cancel = false; // continue to close

        }

        private async void OnImageRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            PlayerModel data = ((Grid)sender).Tag as PlayerModel;
            await LoadNewImage(data);
        }
    }


    public class ColorChoices
    {
        public Color Background { get; set; } = Colors.Blue;
        public Color Foreground { get; set; } = Colors.White;
        public string Name { get; set; } = "Blue";
        public ColorChoices(string name, Color background, Color foreground)
        {
            Background = background;
            Foreground = foreground;
            Name = name;
        }
        public override string ToString()
        {
            return String.Format($"{Name} - {Background}:{Foreground}");
        }
    }
}

