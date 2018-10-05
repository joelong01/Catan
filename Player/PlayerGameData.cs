using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public class PlayerGameData : INotifyPropertyChanged
    {
        private bool[] _RoadTie = new bool[10]; // does this instance win the ties for this count of roads?

        public CardsLostUpdatedHandler OnCardsLost;

        public ObservableCollection<RoadCtrl> Roads { get; set; } = new ObservableCollection<RoadCtrl>();
        public ObservableCollection<RoadCtrl> Ships { get; set; } = new ObservableCollection<RoadCtrl>();
        public ObservableCollection<BuildingCtrl> Settlements { get; set; } = new ObservableCollection<BuildingCtrl>();
        public ObservableCollection<BuildingCtrl> Cities { get; set; } = new ObservableCollection<BuildingCtrl>();
        public PlayerResourceData PlayerResourceData { get; set; } = null;
        private readonly List<string> _savedGameProperties = new List<string> { "PlayerIdentified", "Score", "ResourceCount", "KnightsPlayed","TimesTargeted", "NoResourceCount", "RollsWithResource", "MaxNoResourceRolls", "CardsLost", "CardsLostToSeven", "CardsLostToMonopoly", "ResourcesAcquired",
                                                                       "LargestArmy",  "HasLongestRoad", "Rolls", "ColorAsString", "RoadsLeft", "CitiesPlayed", "SettlementsLeft", "TotalTime",
                                                                        "Roads", "Ships", "Buildings", "Rolls", "PlayedKnightThisTurn", "MovedBaronAfterRollingSeven"};
        private Dictionary<Island, int> _islands = new Dictionary<Island, int>();

        private PlayerData _playerData = null; // back pointer

        public PlayerGameData(PlayerData pData)
        {
            Roads.CollectionChanged += Roads_CollectionChanged;
            Settlements.CollectionChanged += Settlements_CollectionChanged;
            Cities.CollectionChanged += Cities_CollectionChanged;
            Ships.CollectionChanged += Ships_CollectionChanged;
            _playerData = pData;
            PlayerResourceData = new PlayerResourceData(pData);
            PlayerResourceData.OnPlayerResourceUpdate += OnPlayerResourceUpdate;
        }


        private void LogPropertyChanged(string oldVal, string newVal, bool stopUndo = false, [CallerMemberName] String propertyName = "")
        {
            _playerData.Log.PostLogEntry(_playerData, GameState.Unknown,
                                                             CatanAction.ChangedPlayerProperty, stopUndo, LogType.Normal, -1,
                                                             new LogPropertyChanged(propertyName, oldVal, newVal));
        }

        private void LogPropertyChanged(int oldVal, int newVal, bool stopUndo = false, [CallerMemberName] String propertyName = "")
        {
            LogPropertyChanged(oldVal.ToString(), newVal.ToString(), stopUndo, propertyName);
        }

        private void LogPropertyChanged(bool oldVal, bool newVal, bool stopUndo = false, [CallerMemberName] String propertyName = "")
        {
            LogPropertyChanged(oldVal.ToString(), newVal.ToString(), stopUndo, propertyName);
        }
        private void LogPropertyChanged(bool? oldVal, bool? newVal, bool stopUndo = false, [CallerMemberName] String propertyName = "")
        {
            LogPropertyChanged(oldVal.ToString(), newVal.ToString(), stopUndo, propertyName);
        }
        private void OnPlayerResourceUpdate(PlayerData player, ResourceType resource, int oldVal, int newVal)
        {
            _playerData.Log.PostLogEntry(player, GameState.Unknown,
                                                             CatanAction.AddResourceCount, false, LogType.Normal, newVal - oldVal,
                                                             new LogResourceCount(oldVal, newVal, resource));

        }

        private void Ships_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ShipsPlayed = Ships.Count;
        }

        private void Cities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CitiesPlayed = Cities.Count;
            UpdateScore();
            Pips = CalculatePips(Settlements, Cities);
        }

        private void Settlements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SettlementsPlayed = Settlements.Count;
            UpdateScore();
            Pips = CalculatePips(Settlements, Cities);
        }

        public static int CalculatePips(IEnumerable<BuildingCtrl> Settlements, IEnumerable<BuildingCtrl> Cities)
        {
            int pips = 0;
            foreach (BuildingCtrl s in Settlements)
            {
                foreach (KeyValuePair<BuildingLocation, TileCtrl> kvp in s.BuildingToTileDictionary)
                {
                    pips += kvp.Value.Pips;
                }
            }
            foreach (BuildingCtrl s in Cities)
            {
                foreach (KeyValuePair<BuildingLocation, TileCtrl> kvp in s.BuildingToTileDictionary)
                {
                    pips += kvp.Value.Pips * 2;
                }
            }

            return pips;
        }

        private void Roads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RoadsPlayed = Roads.Count();
        }

        public void Reset()
        {
            _islands = new Dictionary<Island, int>();
            Score = 0;
            KnightsPlayed = 0;
            LongestRoad = 0;
            TimesTargeted = 0;
            NoResourceCount = 0;
            RollsWithResource = 0;
            MaxNoResourceRolls = 0;
            GoodRoll = false;
            CardsLost = 0;
            CardsLostToBaron = 0;
            CardsLostToSeven = 0;
            CardsLostToMonopoly = 0;
            ResourcesAcquired = 0;
            LargestArmy = false;
            HasLongestRoad = false;
            RoadsPlayed = 0;
            ShipsPlayed = 0;
            CitiesPlayed = 0;
            SettlementsPlayed = 0;
            IslandsPlayed = 0;
            TotalTime = TimeSpan.FromSeconds(0);
            MovedBaronAfterRollingSeven = null;
            PlayedKnightThisTurn = false;
            Roads.Clear();
            Settlements.Clear();
            Cities.Clear();
            Ships.Clear();
            IsCurrentPlayer = false;
            MaxShips = 0;
            MaxRoads = 0;
            MaxSettlements = 0;
            MaxCities = 0;
            PlayerResourceData.Reset();
            Pips = 0;

            for (int i = 0; i < _RoadTie.Count(); i++)
            {
                _RoadTie[i] = false;
            }

        }

        /// <summary>
        ///     called if this particular player is the first to reach the road count of 5...14
        ///     this is important because this is the Player that wins ties.
        ///     
        /// </summary>
        /// <param name="roadCount"></param>
        public void SetRoadCountTie(int roadCount, bool winTie)
        {
            Debug.Assert(roadCount >= 5 && roadCount <= 15, "bad roadcount");
            _RoadTie[roadCount - 5] = winTie;

        }

        public bool WinsRoadCountTie(int roadCount)
        {
            Debug.Assert(roadCount >= 5 && roadCount <= 15, "bad roadcount");
            return _RoadTie[roadCount - 5];
        }

        public ObservableCollection<RoadCtrl> RoadsAndShips
        {
            get
            {
                ObservableCollection<RoadCtrl> ret = new ObservableCollection<RoadCtrl>();
                ret.AddRange(Roads);
                ret.AddRange(Ships);
                return ret;
            }
        }

        public int CitiesLeft => MaxCities - CitiesPlayed;

        public int SettlementsLeft => MaxSettlements - SettlementsPlayed;

        public int RoadsLeft => MaxRoads - RoadsPlayed;

        public int ShipsLeft => MaxShips - ShipsPlayed;

        public void AddIsland(Island island)
        {

            if (_islands.TryGetValue(island, out int refCount))
            {
                refCount++;
            }
            else
            {
                refCount = 1;
            }
            _islands[island] = refCount;
            UpdateScore();
        }

        public void RemoveIsland(Island island)
        {
            int refCount = _islands[island];
            refCount--;
            if (refCount == 0)
            {
                _islands.Remove(island);

            }
            else
            {
                _islands[island] = refCount;
            }
            IslandsPlayed = _islands.Count;
            UpdateScore();
        }

        private void UpdateScore()
        {
            int score = CitiesPlayed * 2 + SettlementsPlayed;

            score += HasLongestRoad ? 2 : 0;
            score += LargestArmy ? 2 : 0;

            IslandsPlayed = _islands.Count;

            score += _islands.Count;

            Score = score;


        }

        public static string[] NonDisplayData()
        {
            return new string[] { "PlayerIdentifier", "Roads", "Ships", "Buildings", "Rolls" };
        }

        public string Serialize(bool oneLine)
        {

            return StaticHelpers.SerializeObject<PlayerGameData>(this, _savedGameProperties, "=", "|");
        }


        public bool Deserialize(string s, bool oneLine)
        {

            StaticHelpers.DeserializeObject<PlayerGameData>(this, s, ":", "|");
            return true;
        }

        bool _goodRoll = false;
        int _score = 0;
        int _knightsPlayed = 0;
        int _timesTargeted = 0;
        int _noResourceCount = 0;
        int _rollsWithResource = 0;
        int _maxNoResourceRolls = 0;
        int _cardsLost = 0;
        int _CardsLostToSeven = 0;
        int _ResourcesAcquired = 0;
        bool _LargestArmy = false;
        bool _HasLongestRoad = false;
        int _LongestRoad = 0;

        int _RoadsPlayed = 0;
        int _ShipsPlayed = 0;
        int _CitiesPlayed = 0;
        int _SettlementsPlayed = 0;
        TimeSpan _TotalTime = TimeSpan.FromSeconds(0);
        bool? _MovedBaronAfterRollingSeven = null;
        bool _PlayedKnightThisTurn = false;
        Guid _PlayerIdentifier = new Guid();
        int _CardsLostToBaron = 0;
        string _ColorAsString = "HotPink"; // a useful default to pick out visually - you should *NEVER* see this color in the UI

        int _IslandsPlayed = 0;
        bool _isCurrentPlayer = false;
        int _MaxShips = 0;
        int _MaxRoads = 0;
        int _MaxCities = 0;
        int _MaxSettlements = 0;

        bool _useLightFile = true;
        public bool UseLightFile
        {
            get => _useLightFile;
            set
            {
                if (_useLightFile != value)
                {

                    _useLightFile = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //
        //  This is the Color that drives the player UI
        //  this gets set (say) "Red" and it will create the right 2 brushes
        //  to be used by *everything* that wants to show the players colors.
        //
        //  Note:  
        //          NOT a unique identifier, as you can have two players with 
        //          the same color, as least temporarily.
        //
        //  this will set the SolidColorBrush used as Background ("Fill") and Foreground (the color of the shapes)
        //
        //  there should be no other brushes uses for players colors than these two.
        //  
        public string ColorAsString
        {
            get => _ColorAsString;
            set
            {
                if (_ColorAsString != value)
                {
                    LogPropertyChanged(_ColorAsString, value, true); // when you change the color, it will look like something you shoudl be able to undo, hence the "true"
                    _ColorAsString = value;
                    NotifyPropertyChanged();
                    Background = CreateBrushFromResource(value);
                    NotifyPropertyChanged("Background");
                    Foreground = CreateBrushFromResource(StaticHelpers.BackgroundToForegroundDictionary[value]);
                    NotifyPropertyChanged("Foreground");
                    PlayerColor = Background.Color;
                    NotifyPropertyChanged("PlayerColor");
                    if (Foreground.Color != Colors.White)
                    {
                        UseLightFile = true;
                    }
                    else
                    {
                        UseLightFile = false;
                    }


                }
            }
        }
        public SolidColorBrush Foreground { get; private set; } = new SolidColorBrush(Colors.White);  // this is what CastleColor and the like should bind to
        public SolidColorBrush Background { get; private set; } = new SolidColorBrush(Colors.Green); // this is what "Fill" and the like should bind to
        public Color PlayerColor { get; private set; } = Colors.Green;

        private SolidColorBrush CreateBrushFromResource(string color)
        {
            Color c = Colors.HotPink;
            if (StaticHelpers.StringToColorDictionary.TryGetValue(color, out c))
            {
                return new SolidColorBrush(c);
            }

            return null;
        }


        int pips = 0;
        public int Pips
        {
            get => pips;
            set
            {
                if (pips != value)
                {
                    LogPropertyChanged(pips, value); // this needs to be here so that the Pips are set when the log is replayed.  it'd be unfortunate if this was Undone...
                    pips = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int MaxSettlements
        {
            get => _MaxSettlements;
            set
            {
                if (_MaxSettlements != value)
                {
                    _MaxSettlements = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("SettlementsLeft");
                }
            }
        }
        public int MaxCities
        {
            get => _MaxCities;
            set
            {
                if (_MaxCities != value)
                {
                    _MaxCities = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("CitiesLeft");
                }
            }
        }
        public int MaxRoads
        {
            get => _MaxRoads;
            set
            {
                if (_MaxRoads != value)
                {
                    _MaxRoads = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("RoadsLeft");

                }
            }
        }
        public int MaxShips
        {
            get => _MaxShips;
            set
            {
                if (_MaxShips != value)
                {
                    _MaxShips = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("ShipsLeft");
                }
            }
        }
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

        public bool GoodRoll
        {
            get => _goodRoll;
            set
            {
                if (_goodRoll != value)
                {
                    _goodRoll = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int IslandsPlayed
        {
            get => _IslandsPlayed;
            set
            {
                if (_IslandsPlayed != value)
                {
                    _IslandsPlayed = value;
                    NotifyPropertyChanged();
                }
            }
        }



        public int CardsLostToBaron
        {
            get => _CardsLostToBaron;
            set
            {
                if (_CardsLostToBaron != value)
                {
                    LogPropertyChanged(_CardsLostToBaron, value);
                    _CardsLostToBaron = value;
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
        public bool PlayedKnightThisTurn
        {
            get => _PlayedKnightThisTurn;
            set
            {
                if (_PlayedKnightThisTurn != value)
                {
                    _PlayedKnightThisTurn = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool? MovedBaronAfterRollingSeven
        {
            get => _MovedBaronAfterRollingSeven;
            set
            {
                if (_MovedBaronAfterRollingSeven != value)
                {
                    LogPropertyChanged(_MovedBaronAfterRollingSeven, value);
                    _MovedBaronAfterRollingSeven = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public TimeSpan TotalTime
        {
            get => _TotalTime;
            set
            {
                if (_TotalTime != value)
                {
                    _TotalTime = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int SettlementsPlayed
        {
            get => _SettlementsPlayed;
            set
            {
                if (_SettlementsPlayed != value)
                {
                    _SettlementsPlayed = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("SettlementsLeft");
                }
            }
        }
        public int CitiesPlayed
        {
            get => _CitiesPlayed;
            set
            {
                if (_CitiesPlayed != value)
                {
                    _CitiesPlayed = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("CitiesLeft");
                }
            }
        }
        public int RoadsPlayed
        {
            get => _RoadsPlayed;
            set
            {
                if (_RoadsPlayed != value)
                {
                    _RoadsPlayed = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("RoadsLeft");
                }
            }
        }
        public int ShipsPlayed
        {
            get => _ShipsPlayed;
            set
            {
                if (_ShipsPlayed != value)
                {
                    _ShipsPlayed = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("ShipsLeft");
                }
            }
        }


        public int LongestRoad
        {
            get => _LongestRoad;
            set
            {
                if (_LongestRoad != value)
                {
                    _LongestRoad = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool HasLongestRoad
        {
            get => _HasLongestRoad;
            set
            {
                if (_HasLongestRoad != value)
                {
                    _HasLongestRoad = value;
                    NotifyPropertyChanged();
                    UpdateScore();
                }
            }
        }
        public bool LargestArmy
        {
            get => _LargestArmy;
            set
            {
                if (_LargestArmy != value)
                {
                    _LargestArmy = value;
                    NotifyPropertyChanged();
                    UpdateScore();
                }
            }
        }
        public int ResourcesAcquired
        {
            get => _ResourcesAcquired;
            set
            {
                if (_ResourcesAcquired != value)
                {
                    LogPropertyChanged(_ResourcesAcquired, value);
                    _ResourcesAcquired = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int CardsLostToSeven
        {
            get => _CardsLostToSeven;
            set
            {
                if (_CardsLostToSeven != value)
                {
                    LogPropertyChanged(_CardsLostToSeven, value);
                    _CardsLostToSeven = value;
                    NotifyPropertyChanged();
                }
            }
        }

        int _CardsLostToMonopoly = 0;
        public int CardsLostToMonopoly
        {
            get => _CardsLostToMonopoly;
            set
            {
                if (_CardsLostToMonopoly != value)
                {
                    LogPropertyChanged(_CardsLostToMonopoly, value);
                    _CardsLostToMonopoly = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int CardsLost
        {
            get => _cardsLost;
            set
            {
                if (_cardsLost != value)
                {
                    LogPropertyChanged(_cardsLost, value);
                    OnCardsLost?.Invoke(_playerData, _cardsLost, value);
                    _cardsLost = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int TimesTargeted
        {
            get => _timesTargeted;
            set
            {
                if (_timesTargeted != value)
                {
                    LogPropertyChanged(_timesTargeted, value);
                    _timesTargeted = value;
                    NotifyPropertyChanged();
                }
            }
        }



        public int NoResourceCount
        {
            get => _noResourceCount;
            set
            {
                if (_noResourceCount != value)
                {
                    LogPropertyChanged(_noResourceCount, value);
                    _noResourceCount = value;
                    if (value > MaxNoResourceRolls)
                    {
                        MaxNoResourceRolls = value; // only takes on Set
                    }

                    NotifyPropertyChanged();
                }
            }
        }
        public int RollsWithResource
        {
            get => _rollsWithResource;
            set
            {
                if (_rollsWithResource != value)
                {
                    LogPropertyChanged(_rollsWithResource, value);
                    _rollsWithResource = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int MaxNoResourceRolls
        {
            get => _maxNoResourceRolls;
            set
            {
                if (_maxNoResourceRolls != value) // only record the max
                {
                    LogPropertyChanged(_maxNoResourceRolls, value);
                    _maxNoResourceRolls = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int KnightsPlayed
        {
            get => _knightsPlayed;
            set
            {
                if (_knightsPlayed != value)
                {
                    LogPropertyChanged(_knightsPlayed, value);
                    _knightsPlayed = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    _score = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }



        Dictionary<ResourceType, ResourceCount> _dictResourceCount = new Dictionary<ResourceType, ResourceCount>();
        // 
        /// <summary>
        ///     returns the number of resources the user actually got
        ///     this is Acquired and Lost *only*
        /// </summary>
        internal int UpdateResourceCount(ResourceType resourceType, BuildingState buildingState, bool hasBaron, bool undo)
        {
            if (buildingState == BuildingState.None)
            {
                throw new InvalidDataException("the settlement type shouldn't be None!");
            }

            if (_dictResourceCount.TryGetValue(resourceType, out ResourceCount resCount) == false)
            {
                resCount = new ResourceCount();
                _dictResourceCount[resourceType] = resCount;
            }

            int value = 0;
            if (buildingState == BuildingState.Settlement)
            {
                value = 1;
            }
            else if (buildingState == BuildingState.City)
            {
                value = 2;
            }

            if (undo)
            {
                value = -value;
            }

            if (hasBaron)
            {
                resCount.Lost += value;
                CardsLostToBaron += value;
                value = 0; //didn't actually get any resources
            }
            else
            {
                resCount.Acquired += value;
                ResourcesAcquired += value;
            }

            return value;

        }
    }

    /// <summary>
    ///     PlayerResourceData:  a class that keeps track of the number of resources that happen on a per roll basis.
    /// </summary>


    public class PlayerResourceData : INotifyPropertyChanged
    {
        private readonly PlayerData _playerData = null;
        public PlayerResourceUpdateHandler OnPlayerResourceUpdate;


        public PlayerResourceData(PlayerData player)
        {
            _playerData = player;
        }

        public void Reset()
        {
            Gold = 0;
            Wheat = 0;
            Ore = 0;
            Sheep = 0;
            Brick = 0;
            Wood = 0;
        }

        int _Sheep = 0;
        int _Wood = 0;
        int _Ore = 0;
        int _Brick = 0;
        int _Wheat = 0;
        int _Gold = 0;
        public int Gold
        {
            get => _Gold;
            set
            {

                if (_Gold != value)
                {
                    OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.GoldMine, _Gold, value);
                    _Gold = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }
        public int Wheat
        {
            get => _Wheat;
            set
            {

                if (_Wheat != value)
                {
                    OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.Wheat, _Wheat, value);
                    _Wheat = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }
        public int Brick
        {
            get => _Brick;
            set
            {

                if (_Brick != value)
                {
                    OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.Brick, _Brick, value);
                    _Brick = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }
        public int Ore
        {
            get => _Ore;
            set
            {

                if (_Ore != value)
                {
                    OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.Ore, _Ore, value);
                    _Ore = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }
        public int Wood
        {
            get => _Wood;
            set
            {

                if (_Wood != value)
                {
                    OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.Wood, _Wood, value);
                    _Wood = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }
        public int Sheep
        {
            get => _Sheep;
            set
            {

                if (_Sheep != value)
                {
                    OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.Sheep, _Sheep, value);
                    _Sheep = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        internal int AddResourceCount(ResourceType resource, int count)
        {
            int oldVal = 0;
            switch (resource)
            {
                case ResourceType.Sheep:
                    oldVal = Sheep;
                    Sheep += count;
                    break;
                case ResourceType.Wood:
                    oldVal = Wood;
                    Wood += count;
                    break;
                case ResourceType.Ore:
                    oldVal = Ore;
                    Ore += count;
                    break;
                case ResourceType.Wheat:
                    oldVal = Wheat;
                    Wheat += count;
                    break;
                case ResourceType.Brick:
                    oldVal = Brick;
                    Brick += count;
                    break;
                case ResourceType.GoldMine:
                    oldVal = Gold;
                    Gold += count;
                    break;
                default:
                    break;
            }

            return oldVal;
        }

        public int Total => Sheep + Wood + Ore + Wheat + Brick + Gold;


    }
}