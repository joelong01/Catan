using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Catan10.CatanService;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    internal class ResourceCount
    {
        public int Acquired { get; set; } = 0;

        public int Lost { get; set; } = 0;

        public override string ToString()
        {
            return String.Format($"Acquired:{Acquired} Lost:{Lost}");
        }
    }

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
        private Color _Foreground = Colors.White;

        private ImageBrush _imageBrush = null;

        private string _ImageFileName = "ms-appx:///Assets/guest.jpg";

        private PlayerGameModel _playerGameData = null;

        private Guid _PlayerIdentifier;

        private string _playerName = "Nameless";

        private Color _primaryBackgroundColor = Colors.SlateBlue;

        private Color _secondaryBackgroundColor = Colors.Black;
        double _serviceLatency = 0;
        public double ServiceLatency
        {
            get
            {
                return _serviceLatency;
            }
            set
            {
                if (_serviceLatency != value)
                {
                    _serviceLatency = value;
                    NotifyPropertyChanged("LatencyString");
                    NotifyPropertyChanged();

                }
            }
        }

        public string LatencyString
        {
            get
            {
                return $"Latency: {ServiceLatency} ms";
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int PerceivedBrightness(Color c)
        {
            return (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114);
        }

        /// <summary>
        ///     put anything that needs to happen when the turn ended
        /// </summary>
        internal void TurnEnded()
        {
            //
            //  hide the rolls in the public data control
            GameData.Resources.ResourcesThisTurn.Reset(); // reset PublicDataCtrl resources
            GameData.Resources.ThisTurnsDevCard = new DevCardModel() { DevCardType = DevCardType.None };     // flips the "Dev Card Played this turn" in public data ctrl and in PrivateDataCtrl

            GameData.Trades = new Trades();
            GameData.Trades.TradeRequest.Owner.Player = MainPage.Current.TheHuman;
            GameData.Resources.StolenResource = ResourceType.None;
            
        }

        /// <summary>
        ///     put anything that needs to happen when a turn starts here
        /// </summary>
        internal void TurnStarted()
        {
            //
            //  moved your purchased dev cards into the available to play
            GameData.Resources.AvailableDevCards.AddRange(GameData.Resources.NewDevCards);
            GameData.Resources.NewDevCards.Clear();
            GameData.Resources.StolenResource = ResourceType.None;
        }

        //
        //  what time the AddPlayer message was sent
        public DateTime AddedTime { get; set; }

        [JsonIgnore]
        public ObservableCollection<Brush> AvailableColors => new ObservableCollection<Brush>(CatanColors.AllAvailableBrushes());

        [JsonIgnore]
        public LinearGradientBrush BackgroundBrush
        {
            get
            {
                var brush = ConverterGlobals.GetLinearGradientBrush(this.PrimaryBackgroundColor, this.SecondaryBackgroundColor);
           
                return brush;
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
                    NotifyPropertyChanged("GetForegroundBrush");    // this is the binding function name that the roads and buildings use
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

        [JsonIgnore]
        public PlayerModel PlayerDataInstance => this;

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
                    NotifyPropertyChanged("SolidPrimaryBrush");
                    NotifyPropertyChanged("UseWhiteImages");
                    NotifyPropertyChanged("GetBackgroundBrush"); // this is the binding function name that the roads and buildings use
                    NotifyPropertyChanged("GetForegroundBrush");
                }
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
                    NotifyPropertyChanged("UseWhiteImages");
                    NotifyPropertyChanged("SolidSecondaryBrush");
                    NotifyPropertyChanged("GetBackgroundBrush");    // this is the binding function name that the roads and buildings use
                    NotifyPropertyChanged("GetForegroundBrush");
                }
            }
        }

        [JsonIgnore]
        public SolidColorBrush SolidPrimaryBrush
        {
            get
            {
                return ConverterGlobals.GetBrush(PrimaryBackgroundColor);
            }
        }

        [JsonIgnore]
        public SolidColorBrush SolidSecondaryBrush
        {
            get
            {
                return ConverterGlobals.GetBrush(SecondaryBackgroundColor);
            }
        }

        [JsonIgnore]
        public PlayerModel This => this;

        [JsonIgnore]
        public bool UseWhiteImages
        {
            get
            {
                return PerceivedBrightness(PrimaryBackgroundColor) > 130 ? true : false;
            }
        }

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

            GameData = new PlayerGameModel(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static PlayerModel Deserialize(string json)
        {
            return CatanSignalRClient.Deserialize<PlayerModel>(json);
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

        public string Serialize(bool indented)
        {
            return CatanSignalRClient.Serialize<PlayerModel>(this, indented);
        }

        public override string ToString()
        {
            return String.Format($"{PlayerName}");
        }
    }
}
