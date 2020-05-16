using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
    public class ColorChoice
    {
        public string Description { get; set; } = "";
        public Windows.UI.Color Color { get; set; } = Colors.Blue;

        public System.Drawing.Color SDColor
        {
            set
            {
                this.Color = Windows.UI.Color.FromArgb(value.A, value.R, value.G, value.B);
            }
        }

        public override string ToString()
        {
            return Description;
        }

    }

    public static class BrushDependecyFunctions
    {
        public static SolidColorBrush ColorToBrush(Color color)
        {
            return ConverterGlobals.GetBrush(color);
        }
    }


    public sealed partial class PlayerManagementDlg : ContentDialog
    {




        public ObservableCollection<PlayerModel> PlayerDataList { get; } = new ObservableCollection<PlayerModel>();
        public ObservableCollection<ColorChoice> PrimaryColors { get; } = new ObservableCollection<ColorChoice>();
        public ObservableCollection<ColorChoice> SecondaryColors { get; } = new ObservableCollection<ColorChoice>();
        public ObservableCollection<ColorChoice> ForegroundColors { get; } = new ObservableCollection<ColorChoice>();

        
        
        public static readonly DependencyProperty PrimaryColorProperty = DependencyProperty.Register("PrimaryColor", typeof(ColorChoice), typeof(PlayerManagementDlg), new PropertyMetadata(new ColorChoice(), PrimaryColorChanged));
        public static readonly DependencyProperty SecondaryColorProperty = DependencyProperty.Register("SecondaryColor", typeof(ColorChoice), typeof(PlayerManagementDlg), new PropertyMetadata(new ColorChoice(), SecondaryColorChanged));
        public static readonly DependencyProperty ForegroundColorProperty = DependencyProperty.Register("ForegroundColor", typeof(ColorChoice), typeof(PlayerManagementDlg), new PropertyMetadata(new ColorChoice(), ForegroundColorChanged));
        public static readonly DependencyProperty SelectedPlayerProperty = DependencyProperty.Register("SelectedPlayer", typeof(PlayerModel), typeof(PlayerManagementDlg), new PropertyMetadata(null, SelectedPlayerChanged));
        public PlayerModel SelectedPlayer
        {
            get => (PlayerModel)GetValue(SelectedPlayerProperty);
            set => SetValue(SelectedPlayerProperty, value);
        }
        private static void SelectedPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayerManagementDlg;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetSelectedPlayer(depPropValue);
        }
        private void SetSelectedPlayer(PlayerModel player)
        {
            if (player == null) return;

            bool primaryFound = false;
            bool secondaryFound = false;
            bool foregroundFound = false;

            foreach (var choice in PrimaryColors)
            {
                if (choice.Color.Equals(player.PrimaryBackgroundColor))
                {
                    PrimaryColor = choice;
                    primaryFound = true;
                    
                }
                if (choice.Color.Equals(player.SecondaryBackgroundColor))
                {
                    SecondaryColor = choice;
                    secondaryFound = true;

                }
                if (choice.Color.Equals(player.ForegroundColor))
                {
                    ForegroundColor = choice;
                    foregroundFound = true;

                }
                if (primaryFound && foregroundFound && secondaryFound) break;
            }


        }

        public ColorChoice ForegroundColor
        {
            get => (ColorChoice)GetValue(ForegroundColorProperty);
            set => SetValue(ForegroundColorProperty, value);
        }
        private static void ForegroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayerManagementDlg;
            var depPropValue = (ColorChoice)e.NewValue;
            depPropClass?.SetForegroundColor(depPropValue);
        }
        private void SetForegroundColor(ColorChoice value)
        {
            if (SelectedPlayer != null)
            {
                SelectedPlayer.ForegroundColor = value.Color;
            }
        }



        public ColorChoice SecondaryColor
        {
            get => (ColorChoice)GetValue(SecondaryColorProperty);
            set => SetValue(SecondaryColorProperty, value);
        }
        private static void SecondaryColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayerManagementDlg;
            var depPropValue = (ColorChoice)e.NewValue;
            depPropClass?.SetSecondaryColor(depPropValue);
        }
        private void SetSecondaryColor(ColorChoice value)
        {
            if (SelectedPlayer != null)
            {
                SelectedPlayer.SecondaryBackgroundColor = value.Color;
            }
        }



        public ColorChoice PrimaryColor
        {
            get => (ColorChoice)GetValue(PrimaryColorProperty);
            set => SetValue(PrimaryColorProperty, value);
        }
        private static void PrimaryColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayerManagementDlg;
            var depPropValue = (ColorChoice)e.NewValue;
            depPropClass?.SetPrimaryColor(depPropValue);
        }
        private void SetPrimaryColor(ColorChoice value)
        {
            if (SelectedPlayer != null)
            {
                SelectedPlayer.PrimaryBackgroundColor = value.Color;
            }
        }



        public PlayerManagementDlg(ICollection<PlayerModel> playerData)
        {
            this.InitializeComponent();

            foreach (var p in playerData)
            {
                PlayerDataList.Add(p);
            }
            if (PlayerDataList.Count > 0)
            {
                SelectedPlayer = PlayerDataList[0];
            }
            

            var colorNames = Enum.GetValues(typeof(KnownColor));
            foreach (var knownColor in colorNames)
            {
                var sdColor = System.Drawing.Color.FromName(knownColor.ToString());
                if (sdColor.IsKnownColor)
                {
                    var choice = new ColorChoice() { SDColor = sdColor, Description = knownColor.ToString() };
                    PrimaryColors.Add(choice);
                    SecondaryColors.Add(choice);
                    ForegroundColors.Add(choice);
                }
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


        private void OnPlayerColorChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnCurrentPlayerChanged(object sender, SelectionChangedEventArgs e)
        {

            //if (e.AddedItems.Count == 0)
            //{
            //    if (e.RemovedItems.Count > 0)
            //    {
            //        //
            //        //   unselected
            //        SelectedPlayer.GameData.IsCurrentPlayer = false;
            //        SelectedPlayer = null;

            //    }

            //    return;
            //}

            //PlayerModel player = e.AddedItems[0] as PlayerModel; // single select only
            //if (SelectedPlayer != null)
            //{
            //    SelectedPlayer.GameData.IsCurrentPlayer = false;
            //}
            //SelectedPlayer = player;
            //SelectedPlayer.GameData.IsCurrentPlayer = true;
            this.TraceMessage($"Selected {SelectedPlayer}");

        }



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

        private void OnSaveAndclose(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            args.Cancel = false; // continue to close

        }

        private async void OnImageRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            PlayerModel data = ((Grid)sender).Tag as PlayerModel;
            await LoadNewImage(data);
        }
    }



}

