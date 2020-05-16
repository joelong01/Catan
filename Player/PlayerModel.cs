﻿using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void CardsLostUpdatedHandler(PlayerModel player, int oldVal, int newVal);
    public delegate void PlayerResourceUpdateHandler(PlayerModel player, ResourceType resource, int oldVal, int newVal);




    //
    //  this is where data goes that is applicable to players and all games
    //
    //  Only data that is stored on disk should be stored here
    //  
    //  
    public class PlayerModel : INotifyPropertyChanged
    {

        private string _playerName = "Nameless";
        private string _ImageFileName = "ms-appx:///Assets/guest.jpg";
        private PlayerGameModel _playerGameData = null;
        private Guid _PlayerIdentifier;
        private ImageBrush _imageBrush = null;
        

        Color _primaryBackgroundColor = Colors.SlateBlue;
        Color _secondaryBackgroundColor = Colors.Black;
        Color _Foreground = Colors.White;
        public string PlayerName
        {
            get => _playerName;
            set
            {
                if (_playerName != value)
                {
                    _playerName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Guid PlayerIdentifier
        {
            get => _PlayerIdentifier;
            set
            {
                if (_PlayerIdentifier != value)
                {
                    _PlayerIdentifier = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ImageFileName
        {
            get => _ImageFileName;
            set
            {
                if (_ImageFileName != value)
                {
                    _ImageFileName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Color ForegroundColor
        {
            get
            {
                return _Foreground;
            }
            set
            {
                if (_Foreground != value)
                {
                    _Foreground = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("ForegroundBrush");
                }
            }
        }

        [JsonIgnore]
        public SolidColorBrush ForegroundBrush
        {
            get
            {
                return ConverterGlobals.GetBrush(this.ForegroundColor);
            }
        }

        public Color SecondaryBackgroundColor
        {
            get
            {
                return _secondaryBackgroundColor;
            }
            set
            {
                if (_secondaryBackgroundColor != value)
                {
                    _secondaryBackgroundColor = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("BackgroundBrush");
                }
            }
        }
        public Color PrimaryBackgroundColor
        {
            get
            {
                return _primaryBackgroundColor;
            }
            set
            {
                if (_primaryBackgroundColor != value)
                {
                    _primaryBackgroundColor = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("BackgroundBrush");
                }
            }
        }
        [JsonIgnore]
        public LinearGradientBrush BackgroundBrush
        {
            get
            {
                return ConverterGlobals.GetLinearGradientBrush(this.PrimaryBackgroundColor, this.SecondaryBackgroundColor);
            }
        }

        [JsonIgnore]
        public SolidColorBrush SolidBackgroupBrush
        {
            get
            {
                return ConverterGlobals.GetBrush(PrimaryBackgroundColor);
            }
        }

        [JsonIgnore]
        public PlayerModel PlayerDataInstance => this;
        public event PropertyChangedEventHandler PropertyChanged;


        [JsonIgnore]
        public ObservableCollection<Brush> AvailableColors => new ObservableCollection<Brush>(CatanColors.AllAvailableBrushes());

        public PlayerModel()
        {
            if (StaticHelpers.IsInVisualStudioDesignMode)
            {

                BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/DefaultPlayers/guest.jpg", UriKind.RelativeOrAbsolute));
                ImageBrush brush = new ImageBrush
                {
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top,
                    Stretch = Stretch.UniformToFill,
                    ImageSource = bitmapImage
                };
                ImageBrush = brush;
                _imageBrush = new ImageBrush();
                            
            }

            _playerGameData = new PlayerGameModel(this);

        }


        public async Task<StorageFile> CopyImage(StorageFile file)
        {
            StorageFolder folder = await StaticHelpers.GetSaveFolder();
            StorageFolder imageFolder = await folder.CreateFolderAsync("Player Images", CreationCollisionOption.OpenIfExists);
            return await file.CopyAsync(imageFolder, file.Name, NameCollisionOption.ReplaceExisting);
        }

        public async Task LoadImage()
        {
            try
            {
                if (ImageFileName.Contains("ms-appx"))
                {

                    BitmapImage bitmapImage = new BitmapImage(new Uri(ImageFileName, UriKind.RelativeOrAbsolute));
                    ImageBrush brush = new ImageBrush
                    {
                        AlignmentX = AlignmentX.Left,
                        AlignmentY = AlignmentY.Top,
                        Stretch = Stretch.UniformToFill,
                        ImageSource = bitmapImage
                    };
                    ImageBrush = brush;
                    return;
                }


                StorageFolder folder = await StaticHelpers.GetSaveFolder();
                StorageFolder imageFolder = await folder.GetFolderAsync("Player Images");
                StorageFile file = await imageFolder.CreateFileAsync(ImageFileName, CreationCollisionOption.OpenIfExists);

                using (Windows.Storage.Streams.IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap.
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileStream);

                    ImageBrush brush = new ImageBrush
                    {
                        AlignmentX = AlignmentX.Left,
                        AlignmentY = AlignmentY.Top,
                        Stretch = Stretch.UniformToFill,
                        ImageSource = bitmapImage
                    };
                    ImageBrush = brush;

                }
            }
            catch
            {
                if (await StaticHelpers.AskUserYesNoQuestion($"Problem loading player {PlayerName}.\nDelete it?", "yes", "no") == true)
                {

                }
            }

        }

        [JsonIgnore]
        public PlayerModel This => this;



        [JsonIgnore]
        public ImageBrush ImageBrush
        {
            get => _imageBrush;
            set
            {
                if (value != _imageBrush)
                {
                    _imageBrush = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public ImageSource PlayerImageSource
        {
            get => _imageBrush.ImageSource;
            set
            {
                if (_imageBrush.ImageSource != value)
                {
                    _imageBrush.ImageSource = value;
                    NotifyPropertyChanged();
                }
            }
        }




       

        [JsonIgnore]
        public PlayerGameModel GameData
        {
            get => _playerGameData;
            set
            {
                if (_playerGameData != value)
                {
                    _playerGameData = value;
                    NotifyPropertyChanged();
                }
            }
        }

       


      
     
        public string Serialize(bool indented)
        {
            return CatanProxy.Serialize<PlayerModel>(this, indented);
            
        }



        public static PlayerModel Deserialize(string json)
        {
            return CatanProxy.Deserialize<PlayerModel>(json);

        }


        /// <summary>
        ///     I've run into a bunch of problems where I add data (such as the GoldTiles list) but forget to clear it when Reset
        ///     is called.  this means state moves from one game to the next and we get funny results.  so instead i'm going to try
        ///     to just create a new game model when this is reset and see how it goes.
        ///     
        ///    4/13/2020 - Nasty bug here.  by wiping  GameData() we broke all bindings.  4 hours to debug. The change was to start
        ///                a new game by loading the PlayerData from disk in MainPage.LoadPlayerData();
        /// </summary>
        public void Reset()
        {

            GameData = new PlayerGameModel(this);


        }

        public override string ToString()
        {
            return String.Format($"{PlayerName}");
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

    }

    class ResourceCount
    {
        public int Acquired { get; set; } = 0;
        public int Lost { get; set; } = 0;

        public override string ToString()
        {
            return String.Format($"Acquired:{Acquired} Lost:{Lost}");
        }
    }



}