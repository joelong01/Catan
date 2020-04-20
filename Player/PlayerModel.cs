using Catan.Proxy;

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
        private int _GamesPlayed = 0;
        private int _gamesWon = 0;
        private PlayerPosition _PlayerPosition = PlayerPosition.Right;
        private PlayerGameModel _playerGameData = null;
        private Guid _PlayerIdentifier = new Guid();
        private ImageBrush _imageBrush = null;
        private string _colorAsString = "HotPink";
        private bool _isCurrentPlayer = false;
        public static ObservableCollection<ColorChoices> _availableColors = new ObservableCollection<ColorChoices>();
        public event PropertyChangedEventHandler PropertyChanged;

        public int AllPlayerIndex { get; set; } = -1;

        [JsonIgnore]
        public ILog Log { get; set; } = null;

        [JsonIgnore]
        public bool IsCurrentPlayer
        {
            get => _isCurrentPlayer;
            set
            {
                if (_isCurrentPlayer != value)
                {
                    _isCurrentPlayer = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public PlayerModel PlayerDataInstance => this;
        public PlayerModel()
        {
            //_playerGameData = new PlayerGameModel(this);
            //_playerGameData.ColorAsString = "Blue";

        } // needed for serialization


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
        public ObservableCollection<ColorChoices> AvailableColors => PlayerModel._availableColors;


        public PlayerModel(ILog log)
        {
            GameData = new PlayerGameModel(this);
            Log = log;

            if (_availableColors.Count == 0)
            {
                foreach (KeyValuePair<string, Color> kvp in StaticHelpers.StringToColorDictionary)
                {
                    ColorChoices choice = new ColorChoices(kvp.Key, kvp.Value, StaticHelpers.BackgroundToForegroundColorDictionary[kvp.Value]);
                    _availableColors.Add(choice);
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
        public PlayerPosition PlayerPosition
        {
            get => _PlayerPosition;
            set
            {
                if (_PlayerPosition != value)
                {
                    _PlayerPosition = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [JsonIgnore]
        public int GamesWon
        {
            get => _gamesWon;
            set
            {
                if (_gamesWon != value)
                {
                    _gamesWon = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [JsonIgnore]
        public int GamesPlayed
        {
            get => _GamesPlayed;
            set
            {
                if (_GamesPlayed != value)
                {
                    _GamesPlayed = value;
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
        //
        //  the color that is saved

        public string ColorAsString
        {
            get => _colorAsString;
            set
            {
                // if (value != _colorAsString)
                {
                    //
                    //  we need to tell the GameData what the new color is because
                    //  everything binds to the GameData -- NOTHING should bind to 
                    //  PlayerData.ColorAsString, except for managment UI that wants
                    //  to change the color
                    if (GameData == null) // this happens when we deserialize
                    {
                        GameData = new PlayerGameModel(this);
                    }
                    GameData.ColorAsString = value;
                    _colorAsString = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public string Serialize(bool oneLine)
        {
            return JsonSerializer.Serialize<PlayerModel>(this);
            // return StaticHelpers.SerializeObject<PlayerModel>(this, _savedProperties, "=", StaticHelpers.lineSeperator);
        }



        public static PlayerModel Deserialize(string s, bool oneLine)
        {
            return JsonSerializer.Deserialize<PlayerModel>(s);

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
            return String.Format($"{PlayerName}.{ColorAsString}.{PlayerPosition}");
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        /// <summary>
        ///     when I serialize, I assume
        ///     1. the board is set the same    
        ///         => so I can store the index of roads, settlements, etc. and just set them
        ///     2. we don't replay rolls, so all resource data is saved/loaded
        /// </summary>
        /// <returns></returns>
        internal string Serialize()
        {

            //  var s = JsonConvert.SerializeObject(this);
            //  Debug.WriteLine(s);
            // return s;
            throw new NotImplementedException();

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