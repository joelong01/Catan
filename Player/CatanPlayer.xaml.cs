using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    public delegate void PlayerSelectedHandler(object sender, EventArgs e);

    public sealed partial class CatanPlayer : UserControl, IComparable, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(int), typeof(CatanPlayer), new PropertyMetadata(0));
        public static readonly DependencyProperty ResourceCountProperty = DependencyProperty.Register("ResourceCount", typeof(int), typeof(CatanPlayer), new PropertyMetadata(0));

        string _imageFileName = "";

        int _timesTargeted = 0; // how many times a card was taken from me 
        int _cardsLostToMonopoly = 0; // how many cards I lost because of the Baron
        int _cardsLostToSeven = 0;

        public double CurrentAngle { get { return (double)_daRotateControl.To; } }

        public string PlayerName { get; set; } = "Nameless";
        public string GamesPlayed { get; set; } = "0";
        public Color Color { get; set; } = Colors.Coral;
        public List<RoadCtrl> Roads { get; } = new List<RoadCtrl>();
        public List<SettlementCtrl> Settlements { get; } = new List<SettlementCtrl>();
        public int KnightsPlayed { get; set; } = 0;

        public string ColorAsString
        {
            get
            {
                foreach (var kvp in StaticHelpers.StringToColorDictionary)
                {
                    if (Color == kvp.Value)
                        return kvp.Key;
                }

                this.TraceMessage("Bad FillColor!");
                return "Black";
            }
            set
            {
                Color = StaticHelpers.StringToColorDictionary[value];
            }
        }

        public int Score
        {
            get
            {
                return (int)GetValue(ScoreProperty);
            }
            set
            {
                if (value != Score)
                {
                    SetValue(ScoreProperty, value);

                }

                _gridScore.Visibility = value == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public int ResourceCount
        {
            get
            {
                return (int)GetValue(ResourceCountProperty);
            }
            set
            {
                if (value != ResourceCount)
                {
                    SetValue(ResourceCountProperty, value);
                }

                _gridResourceCount.Visibility = value == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public string ImageFileName
        {
            get
            {
                return _imageFileName;
            }
            set
            {
                _imageFileName = value;
            }
        }
        public string GamesWon { get; set; } = "0";
        public event PlayerSelectedHandler OnPlayerSelected;
        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> _savedProperties = new List<string> { "GamesWon", "GamesPlayed", "PlayerName", "ImageFileName", "ColorAsString" };
        private List<string> _savedGameProperties = new List<string> { "TimesTargeted", "CardsLostToMonopoly", "TotalTime", "CardsLostToSeven", "Rolls", "ColorAsString", "KnightsPlayed", "LongestRoad" };
        List<int> _rolls = new List<int>(); // a useful cache of all the rolls the players have made        
        bool _longestRoad = false;


        public List<int> Rolls
        {
            get
            {
                return _rolls;
            }
            set
            {
                _rolls = value;
            }
        }

        public int LongestRoadCount { get; set; } = 0;

        public bool LongestRoad
        {
            get
            {
                return _longestRoad;
            }

            set
            {
                _tbLongestRoad.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;

                if (value != _longestRoad)
                {

                    if (value)
                    {
                        Score += 2;
                    }
                    else
                    {
                        Score -= 2;
                    }

                    _longestRoad = value;
                }

            }
        }

        bool _largestArmy = false;
        public bool LargestArmy
        {
            get
            {
                return _largestArmy;
            }

            set
            {

                if (_largestArmy != value)
                {
                    _largestArmy = value;
                    if (_largestArmy)
                    {
                        LayoutRoot.Background = new SolidColorBrush(Colors.Red);
                        Score += 2;
                    }
                    else
                    {
                        LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
                        Score -= 2;

                    }
                }
            }
        }


        void Init()
        {
            TimesTargeted = 0;
            CardsLostToMonopoly = 0;
            CardsLostToSeven = 0;
            MissedOpportunity = 0;
            this.Style = (Style)App.Current.Resources["CatanPlayerStyle"];
            SetupTransform();
            _gridTimer.Visibility = Visibility.Collapsed;
            Score = 0;
            ResourceCount = 0;
        }

        public CatanPlayer()
        {
            this.InitializeComponent();
            Init();

        }



        public TimeSpan TotalTime
        {
            get
            {
                return _stopWatch.TotalTime;
            }
            set
            {
                _stopWatch.TotalTime = value;
                VisibleTimer = true;
            }
        }

        public override string ToString()
        {
            return String.Format($"{PlayerName} : {Color}");
        }

        public string Serialize(bool oneLine)
        {

            return StaticHelpers.SerializeObject<CatanPlayer>(this, _savedProperties, oneLine);
        }


        public bool Deserialize(string s, bool oneLine)
        {

            StaticHelpers.DeserializeObject<CatanPlayer>(this, s, oneLine);
            return true;
        }
        public string GameSerialize()
        {
            return StaticHelpers.SerializeObject<CatanPlayer>(this, _savedGameProperties, false);
        }
        public bool DeserializeGame(string s)
        {
            StaticHelpers.DeserializeObject<CatanPlayer>(this, s, false);
            return true;
        }


        public async Task LoadImage()
        {

            if (ImageFileName.Contains("ms-appx:Assets"))
            {

                BitmapImage bitmapImage = new BitmapImage(new Uri(ImageFileName, UriKind.RelativeOrAbsolute));
                ImageBrush brush = new ImageBrush();
                brush.AlignmentX = AlignmentX.Left;
                brush.AlignmentY = AlignmentY.Top;
                brush.Stretch = Stretch.UniformToFill;
                _picBrush.ImageSource = bitmapImage;
                return;
            }

            var folder = await StaticHelpers.GetSaveFolder();
            var file = await folder.GetFileAsync(ImageFileName);

            using (Windows.Storage.Streams.IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap.
                Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage =
                    new Windows.UI.Xaml.Media.Imaging.BitmapImage();

                bitmapImage.SetSource(fileStream);
                _picBrush.ImageSource = bitmapImage;

            }

        }

        public ImageSource PlayerImageSource
        {
            get
            {
                return _picBrush.ImageSource;
            }
            set
            {
                _picBrush.ImageSource = value;
            }
        }

        public Visibility IndexVisibility
        {
            get
            {
                return _tbIndex.Visibility;
            }
            set
            {
                _tbIndex.Visibility = value;
            }
        }

        public string Index
        {
            get
            {
                return _tbIndex.Text;
            }
            set
            {
                _tbIndex.Text = value;

            }
        }

        public int PlayerNumber
        {
            get
            {
                return Int32.Parse(_tbIndex.Text);
            }
        }

        public int TimesTargeted
        {
            get
            {
                return _timesTargeted;
            }

            set
            {

                _timesTargeted = value;
                _gridTimesTargeted.Visibility = (_timesTargeted == 0) ? Visibility.Collapsed : Visibility.Visible;
                _txtCardsTaken.Text = _timesTargeted.ToString();
                NotifyPropertyChanged();

            }
        }

        public int CardsLostToMonopoly
        {
            get
            {
                return _cardsLostToMonopoly;
            }

            set
            {
                _cardsLostToMonopoly = value;
                _gridCardsLostToMonopoly.Visibility = _cardsLostToMonopoly == 0 ? Visibility.Collapsed : Visibility.Visible;
                _txtCardsLostToMonopoly.Text = _cardsLostToMonopoly.ToString();
                NotifyPropertyChanged();
            }
        }

        public int CardsLostToSeven
        {
            get
            {
                return _cardsLostToSeven;
            }

            set
            {
                _cardsLostToSeven = value;
                _gridCardsLostToSeven.Visibility = _cardsLostToSeven == 0 ? Visibility.Collapsed : Visibility.Visible;
                _txtCardsLostToSeven.Text = _cardsLostToSeven.ToString();
                NotifyPropertyChanged();
            }
        }
        int _missedOpportunity = 0;
        public int MissedOpportunity
        {
            get
            {
                return _missedOpportunity;
            }

            set
            {
                _missedOpportunity = value;
                _gridMissedOppportunity.Visibility = _missedOpportunity == 0 ? Visibility.Collapsed : Visibility.Visible;
                _txtMissedOpportunity.Text = _missedOpportunity.ToString();
                NotifyPropertyChanged();
            }
        }

        public void SetCardCountForAction(CatanAction action, int delta)
        {
            switch (action)
            {
                case CatanAction.Rolled:
                case CatanAction.Targeted:
                case CatanAction.ChangedState:
                case CatanAction.ChangedPlayer:
                case CatanAction.MovedToNextPlayer:
                case CatanAction.MovedToPrevPlayer:
                case CatanAction.Dealt:
                    this.Assert(false, "Bad action in SetCardCount");
                    break;
                case CatanAction.CardsLostToMonopoly:
                    this.CardsLostToMonopoly += delta;
                    break;
                case CatanAction.CardsLostToSeven:
                    this.CardsLostToSeven += delta;
                    break;
                case CatanAction.MissedOpportunity:
                    this.MissedOpportunity += delta;
                    break;
                default:
                    break;
            }
        }

        public bool VisibleTimer
        {

            get
            {
                return _gridTimer.Visibility == Visibility.Visible;
            }
            set
            {
                _gridTimer.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }



        public void ResetRotation()
        {
            RotateToAsync(27, 0, 0);
        }

        public void SetupTransform(double CenterX, double TranslateY, double Rotation)
        {

            SetupTransform();


        }

        internal void SetupTransform()
        {
            SetupTransform2(220, 0, 30, 250);
        }

        internal void SetupTransform2(double centerX, double centerY, double translateX, double translateY)
        {
            _gridTransform.CenterX = centerX;
            _gridTransform.CenterY = centerY;
            _gridTransform.TranslateY = translateY;
            _gridTransform.TranslateX = translateX;
            RotateToAsync(0, 0, 0);
        }

        private void SetupRotationAnimation(double angle, double duration, double startAfter)
        {
            _daRotatePlayer.Duration = TimeSpan.FromMilliseconds(duration);
            _daRotatePlayer.BeginTime = TimeSpan.FromMilliseconds(startAfter);
            _daRotateControl.Duration = TimeSpan.FromMilliseconds(duration);
            _daRotateControl.BeginTime = TimeSpan.FromMilliseconds(startAfter);
            _daRotateControl.To = angle;
            _daRotatePlayer.To = -angle;

        }
        public void RotateToAsync(double angle, double duration, double startAfter)
        {
            SetupRotationAnimation(angle, duration, startAfter);
            _sbRotatePlayer.Begin();
            _sbRotateControl.Begin();


        }

        public async Task RotateTo(double angle, double duration, double startAfter)
        {

            SetupRotationAnimation(angle, duration, startAfter);
            Task[] tasks = new Task[2];
            tasks[0] = _sbRotatePlayer.ToTask();
            tasks[1] = _sbRotateControl.ToTask();
            await Task.WhenAll(tasks);
        }

        public List<Task> RotateToTask(double angle, double duration, double startAfter)
        {
            List<Task> taskList = new List<Task>();
            SetupRotationAnimation(angle, duration, startAfter);
            taskList.Add(_sbRotatePlayer.ToTask());
            taskList.Add(_sbRotateControl.ToTask());
            return taskList;

        }

        private void Player_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            OnPlayerSelected?.Invoke(this, EventArgs.Empty);
        }

        public int CompareTo(object obj)
        {
            CatanPlayer p = obj as CatanPlayer;
            return String.Compare(p.PlayerName, this.PlayerName);
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        internal void StopTimer()
        {
            _stopWatch.StopTimer();
        }


        internal void StartTimer()
        {
            _stopWatch.StartTimer();
            VisibleTimer = true;



        }

        Dictionary<ResourceType, ResourceCount> _dictResourceCount = new Dictionary<ResourceType, ResourceCount>();
        internal void UpdateResourceCount(ResourceType resourceType, SettlementType settlementType, bool hasBaron)
        {
            if (settlementType == SettlementType.None)
            {
                throw new InvalidDataException("the settlement type shouldn't be None!");
            }

            ResourceCount resCount = null;
            if (_dictResourceCount.TryGetValue(resourceType, out resCount) == false)
            {
                resCount = new ResourceCount();
                _dictResourceCount[resourceType] = resCount;
            }

            int value = 0;
            if (settlementType == SettlementType.Settlement)
                value = 1;
            else if (settlementType == SettlementType.City)
                value = 2;

            if (hasBaron)
            {
                resCount.Lost += value;
                MissedOpportunity += value;
            }
            else
            {
                resCount.Acquired += value;
                ResourceCount += value;
            }

            string s = hasBaron ? "Lost" : "Gained";

            this.TraceMessage($"{this.PlayerName} {s} {value} of {resourceType} ");
        }

        internal void AddSettlement(SettlementCtrl settlement)
        {


            Settlements.Add(settlement);
        }
        internal void RemoveSettlement(SettlementCtrl settlement)
        {

            Settlements.Remove(settlement);


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
