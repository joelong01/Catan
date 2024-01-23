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
using Windows.ApplicationModel;
using System.Collections.Generic;

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
        //
        /// <summary>
        ///     a default player model for UI binding in design mode
        /// </summary>

        public static PlayerModel DefaultPlayer { get; } = new PlayerModel()
        {

        };

        private Color _Foreground = Colors.White;
        private Color _primaryBackgroundColor = Colors.Blue;
        private string _ImageFileName = "ms-appx:///Assets/guest.jpg";
        private Color _secondaryBackgroundColor = Colors.Black;
        private string _playerName = "Nameless";
        private ImageBrush _imageBrush = null;

        private PlayerGameModel _playerGameData = null;

        private Guid _PlayerIdentifier = Guid.Empty;

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
            return ( int )Math.Sqrt(
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
                try
                {
                    if (DesignMode.DesignModeEnabled)
                    {

                        return ConverterGlobals.CreateLinearGradiantBrush(Colors.Green, Colors.White);

                    }
                    var brush = ConverterGlobals.GetLinearGradientBrush(this.PrimaryBackgroundColor, this.SecondaryBackgroundColor);

                    return brush;
                }
                catch
                {
                    return ConverterGlobals.CreateLinearGradiantBrush(PrimaryBackgroundColor, SecondaryBackgroundColor);
                }
            }
        }

        [JsonIgnore]
        public SolidColorBrush ForegroundBrush
        {
            get
            {

                try
                {
                    if (DesignMode.DesignModeEnabled)
                    {
                        return new SolidColorBrush(Colors.White);
                    }

                    return ConverterGlobals.GetBrush(this.ForegroundColor);
                }
                catch
                {
                    return new SolidColorBrush(Colors.HotPink);
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

        public bool IsWhite
        {
            get => ForegroundColor == Colors.White;
            set
            {
                if (value)
                {
                    ForegroundColor = Colors.White;
                }
                else
                {
                    ForegroundColor = Colors.Black; // Or other default color
                }
                NotifyPropertyChanged("IsWhite");
                NotifyPropertyChanged("IsBlack");
            }
        }

        public bool IsBlack
        {
            get => ForegroundColor == Colors.Black;
            set
            {
                if (value)
                {
                    ForegroundColor = Colors.Black;
                }
                else
                {
                    ForegroundColor = Colors.White; 
                }
                NotifyPropertyChanged("IsWhite");
                NotifyPropertyChanged("IsBlack");
            }
        }

        public PlayerModel()
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
            //  1/2/2024:  found this -- how did this ever work?

            //_imageBrush = new ImageBrush();

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

        public int CalculateLongestRoad()
        {
            int max = 0;
            RoadCtrl maxRoadStartedAt = null;
            foreach (RoadCtrl startRoad in GameData.RoadsAndShips)
            {
                {
                    int count = CalculateLongestRoad(startRoad, new List<RoadCtrl>(), null);
                    if (count > max)
                    {
                        max = count;
                        maxRoadStartedAt = startRoad;
                        if (max == GameData.Roads.Count) // the most roads you can have…only count once
                        {
                            break;
                        }
                    }
                }
            }
          //  this.TraceMessage($"[LongestRoad={max} [Name={PlayerName}] [Start={maxRoadStartedAt?.Index}]");
            return max;
        }
        //
        //  Start is just any old road you want to start counting from
        //  counted are all the roads that have been counted so far -- presumably starts with .Count = 0
        //  blockedFork roads is set when we recurse so that we can pick a direction.  we need it in case of closed loops
        private int CalculateLongestRoad(RoadCtrl start, List<RoadCtrl> counted, RoadCtrl blockedFork)
        {
            int count = 1;
            int max = 1;
            counted.Add(start); // it is counted in the "max=1" above
            RoadCtrl next = start;
            List<RoadCtrl> ownedAdjacentNotCounted = next.OwnedAdjacentRoadsNotCounted(counted, blockedFork, out bool adjacentFork);
            do
            {
                switch (ownedAdjacentNotCounted.Count)
                {
                    case 0:
                        return max;

                    case 1:
                        {
                            count++;
                            next = ownedAdjacentNotCounted[0];
                            counted.Add(next);                  // we counted it, add it to the counted list.

                            if (count > max)
                            {
                                max = count;
                            }

                            ownedAdjacentNotCounted = next.OwnedAdjacentRoadsNotCounted(counted, blockedFork, out adjacentFork);
                            if (adjacentFork)
                            {
                                //ah...the loop
                                count++;
                                counted.Add(next); // we shouldn't have to do this more than once
                                if (count > max)
                                {
                                    max = count;
                                }

                                return max;
                            }
                        }
                        //
                        //  loop to the next road to see if it terminates, forks, or just continues...
                        break;

                    default:

                        //
                        //   general strategy:  for each fork in the road, pretend that all but one of the forks are already counted
                        //                      then count the remaining one.  after that, pick another to be counted
                        //                      because we "count" the entered line, there are only ever 2 forks in the road

                        // ownedAdjacentNotCounted.Count > 1
                        //  usually there means there is a fork like this

                        //                           /
                        //                          /    <=== fork1
                        //                         /
                        //                  ------     <=== always counted
                        //                         \
                        //                          \   <=== Fork 2
                        //                           \

                        //  if we ever get this or the equivalent:
                        //
                        //                           /
                        //                          /    <=== fork1
                        //                         /
                        //                  ------     <=== always counted
                        //                /        \
                        //   Fork 3 -->  /          \   <=== Fork 2
                        //              /            \
                        //
                        //  e.g the adjacent count is > 2 then the road with all the forks around it (the horizontal in ascii art) doesn't have to be counted because we'll count all the
                        //  roads coming into that fork

                        List<RoadCtrl> forks = new List<RoadCtrl>();
                        forks.AddRange(ownedAdjacentNotCounted);
                        if (forks.Count > 2)
                        {
                            //
                            //  if the fork count is not 2 then that means we are in a middle segment, and we don't need to start there
                            return max;
                        }
                        foreach (RoadCtrl road in ownedAdjacentNotCounted)
                        {
                            forks.Remove(road);// now the list has everything except this one road...so we've effectively picked a direction
                            int forkCount = CalculateLongestRoad(road, counted, forks[0]); // --> only one element in the forks list at this point

                            if (count + forkCount > max)
                            {
                                max = count + forkCount;
                            }

                            forks.Add(road); // put fork back so we can count that fork
                        }

                        return max;
                }
            } while (ownedAdjacentNotCounted.Count != 0);

            return max;
        }
    }
}
