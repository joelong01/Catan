using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using Color = Windows.UI.Color;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public static class BrushDependecyFunctions
    {
        public static SolidColorBrush ColorToBrush(Color color)
        {
            return ConverterGlobals.GetBrush(color);
        }

        public static Brush CurrentColorToBrush(Color color, PlayerManagementDlg dlg)
        {
            var player = dlg.SelectedPlayer;
            if (player == null) return ConverterGlobals.GetBrush(color);

            if (dlg.IsPrimaryChecked)
            {
                player.PrimaryBackgroundColor = color;
            }
            if (dlg.IsForegroundChecked)
            {
                player.ForegroundColor = color;
            }
            if (dlg.IsSecondaryChecked)
            {
                player.SecondaryBackgroundColor = color;
            }

            return ConverterGlobals.GetBrush(color);
        }

        public static Color CurrentPlayerToCorrectColor(PlayerManagementDlg dlg)
        {
            var player = dlg.SelectedPlayer;
            if (player == null) return Colors.HotPink;
            if (dlg.IsPrimaryChecked)
            {
                return player.PrimaryBackgroundColor;
            }
            if (dlg.IsForegroundChecked)
            {
                return player.ForegroundColor;
            }
            if (dlg.IsSecondaryChecked)
            {
                return player.SecondaryBackgroundColor;
            }
            return Colors.HotPink;
        }
    }

    public sealed partial class PlayerManagementDlg : ContentDialog
    {
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

        private async void OnAddPlayer(object sender, RoutedEventArgs e)
        {
            PlayerModel pd = new PlayerModel();
            await pd.LoadImage();
            PlayerDataList.Add(pd);
            _gvPlayers.ScrollIntoView(pd);
        }

        private void OnDeletePlayer(object sender, RoutedEventArgs e)
        {
            if (SelectedPlayer != null)
            {
                PlayerDataList.Remove(SelectedPlayer);
                SelectedPlayer = PlayerDataList.FirstOrDefault();
                SelectedPlayer.GameData.IsCurrentPlayer = true;
                _gvPlayers.ScrollIntoView(SelectedPlayer);
            }
        }

        private async void OnImageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            PlayerModel data = ((Grid)sender).Tag as PlayerModel;
            await LoadNewImage(data);
        }

        private async void OnImageRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            PlayerModel data = ((Grid)sender).Tag as PlayerModel;
            await LoadNewImage(data);
        }

        private void OnSaveAndclose(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = false; // continue to close
        }

        private void OnSavePlayer(object sender, RoutedEventArgs e)
        {
        }

        public bool IsForegroundChecked
        {
            get => (bool)GetValue(IsForegroundCheckedProperty);
            set => SetValue(IsForegroundCheckedProperty, value);
        }

        public bool IsPrimaryChecked
        {
            get { return (bool)GetValue(IsPrimaryCheckedProperty); }
            set { SetValue(IsPrimaryCheckedProperty, value); }
        }

        public bool IsSecondaryChecked
        {
            get => (bool)GetValue(IsSecondaryCheckedProperty);
            set => SetValue(IsSecondaryCheckedProperty, value);
        }

        public ObservableCollection<PlayerModel> PlayerDataList { get; } = new ObservableCollection<PlayerModel>();

        public PlayerModel SelectedPlayer
        {
            get => (PlayerModel)GetValue(SelectedPlayerProperty);
            set => SetValue(SelectedPlayerProperty, value);
        }

        public PlayerManagementDlg Self
        {
            get => (PlayerManagementDlg)GetValue(SelfProperty);
            set => SetValue(SelfProperty, value);
        }

        public static readonly DependencyProperty IsForegroundCheckedProperty = DependencyProperty.Register("IsForegroundChecked", typeof(bool), typeof(PlayerManagementDlg), new PropertyMetadata(false));
        public static readonly DependencyProperty IsPrimaryCheckedProperty = DependencyProperty.Register("IsPrimaryChecked", typeof(bool), typeof(PlayerManagementDlg), new PropertyMetadata(true));
        public static readonly DependencyProperty IsSecondaryCheckedProperty = DependencyProperty.Register("IsSecondaryChecked", typeof(bool), typeof(PlayerManagementDlg), new PropertyMetadata(false));
        public static readonly DependencyProperty SelectedPlayerProperty = DependencyProperty.Register("SelectedPlayer", typeof(PlayerModel), typeof(PlayerManagementDlg), new PropertyMetadata(null));
        public static readonly DependencyProperty SelfProperty = DependencyProperty.Register("Self", typeof(PlayerManagementDlg), typeof(PlayerManagementDlg), new PropertyMetadata(null));

        public PlayerManagementDlg(ICollection<PlayerModel> playerData)
        {
            this.InitializeComponent();
            Self = this;

            foreach (var p in playerData)
            {
                PlayerDataList.Add(p);
            }
            if (PlayerDataList.Count > 0)
            {
                SelectedPlayer = PlayerDataList[0];
            }
        }

        public Brush ColorToBrush(Color color)
        {
            return ConverterGlobals.GetBrush(color);
        }

        public Brush GetCurrentBrush()
        {
            Color color = GetCurrentColor();
            return ConverterGlobals.GetBrush(color);
        }

        public Color GetCurrentColor()
        {
            var player = SelectedPlayer;
            if (player == null) return Colors.HotPink;

            if (IsPrimaryChecked)
            {
                return player.PrimaryBackgroundColor;
            }
            if (IsForegroundChecked)
            {
                return player.ForegroundColor;
            }
            if (IsSecondaryChecked)
            {
                return player.SecondaryBackgroundColor;
            }

            return Colors.HotPink;
        }

        public void SetCurrentColor(Color color)
        {
            var player = SelectedPlayer;
            if (player == null) return;

            if (IsPrimaryChecked)
            {
                player.PrimaryBackgroundColor = color;
            }
            if (IsForegroundChecked)
            {
                player.ForegroundColor = color;
            }
            if (IsSecondaryChecked)
            {
                player.SecondaryBackgroundColor = color;
            }
        }
    }
}
