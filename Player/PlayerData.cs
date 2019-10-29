using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void CardsLostUpdatedHandler(PlayerData player, int oldVal, int newVal);
    public delegate void PlayerResourceUpdateHandler(PlayerData player, ResourceType resource, int oldVal, int newVal);




    //
    //  this is where data goes that is applicable to players and all games
    //
    //  Only data that is stored on disk should be stored here
    //  
    //  
    public class PlayerData : INotifyPropertyChanged
    {
        string _playerName = "Nameless";
        string _ImageFileName = "ms-appx:///Assets/guest.jpg";

        int _GamesPlayed = 0;
        int _gamesWon = 0;
        PlayerPosition _PlayerPosition = PlayerPosition.Right;
        PlayerGameData _playerGameData = null;
        Guid _PlayerIdentifier = new Guid();
        ImageBrush _imageBrush = null;
        public int AllPlayerIndex { get; set; } = -1;
        string _colorAsString = "HotPink";

        public ILog Log { get; private set; } = null;


        public static ObservableCollection<ColorChoices> _availableColors = new ObservableCollection<ColorChoices>();
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly List<string> _savedProperties = new List<string> { "PlayerIdentifier", "GamesWon", "GamesPlayed", "PlayerName", "ImageFileName", "ColorAsString" };
        bool _isCurrentPlayer = false;


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

        public PlayerData PlayerDataInstance => this;
        private PlayerData() { } // no default ctor

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

        public PlayerData This => this;

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
        public ObservableCollection<ColorChoices> AvailableColors => PlayerData._availableColors;


        public PlayerData(ILog log)
        {
            _playerGameData = new PlayerGameData(this);
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
        public PlayerGameData GameData
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
                if (value != _colorAsString)
                {
                    //
                    //  we need to tell the GameData what the new color is because
                    //  everything binds to the GameData -- NOTHING should bind to 
                    //  PlayerData.ColorAsString, except for managment UI that wants
                    //  to change the color

                    GameData.ColorAsString = value;
                    _colorAsString = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public string Serialize(bool oneLine)
        {

            return StaticHelpers.SerializeObject<PlayerData>(this, _savedProperties, "=", StaticHelpers.lineSeperator);
        }


        public bool Deserialize(string s, bool oneLine)
        {

            StaticHelpers.DeserializeObject<PlayerData>(this, s, "=", StaticHelpers.lineSeperator);
            return true;
        }


        public void Reset()
        {
            GameData.Reset();
        }

        public override string ToString()
        {
            return String.Format($"{PlayerName}.{ColorAsString}.{PlayerPosition}");
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