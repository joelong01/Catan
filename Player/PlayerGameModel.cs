using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

using Windows.UI.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public class PlayerGameModel : INotifyPropertyChanged
    {
        private readonly bool[] _RoadTie = new bool[10]; // does this instance win the ties for this count of roads?

        private readonly List<string> _savedGameProperties = new List<string> { "Score", "ResourceCount", "KnightsPlayed","TimesTargeted", "NoResourceCount", "RollsWithResource",
                                                                                "MaxNoResourceRolls", "CardsLost", "CardsLostToSeven", "CardsLostToMonopoly", "ResourcesAcquired",
                                                                                "LargestArmy",  "HasLongestRoad", "Rolls", "ColorAsString", "RoadsLeft", "CitiesPlayed", "SettlementsLeft", "TotalTime",
                                                                                "Roads", "Ships", "Buildings", "Rolls", "PlayedKnightThisTurn", "MovedBaronAfterRollingSeven"};

        private int _cardsLost = 0;
        private int _CardsLostToBaron = 0;
        private int _CardsLostToMonopoly = 0;
        private int _CardsLostToSeven = 0;
        private int _CitiesPlayed = 0;
        private List<List<int>> _GoldRolls = new List<List<int>>();

        private bool _goodRoll = false;
        private bool _HasLongestRoad = false;
        private bool _isCurrentPlayer = false;
        private Dictionary<Island, int> _islands = new Dictionary<Island, int>();
        private int _IslandsPlayed = 0;
        private int _knightsPlayed = 0;
        private bool _LargestArmy = false;
        private int _LongestRoad = 0;
        private int _MaxCities = 0;
        private int _maxNoResourceRolls = 0;
        private int _MaxRoads = 0;
        private int _MaxSettlements = 0;
        private int _MaxShips = 0;
        private bool? _MovedBaronAfterRollingSeven = null;
        private int _noResourceCount = 0;
        private bool _PlayedKnightThisTurn = false;
        private PlayerModel _playerData = null;
        private PlayerResources _resources = new PlayerResources();
        private int _RoadsPlayed = 0;
        private TileOrientation _RollOrientation = TileOrientation.FaceDown;
        private int _rollsWithResource = 0;
        private int _score = 0;
        private int _SettlementsPlayed = 0;
        private int _ShipsPlayed = 0;

        private int _timesTargeted = 0;
        private TimeSpan _TotalTime = TimeSpan.FromSeconds(0);
        private int pips = 0;
        public CardsLostUpdatedHandler OnCardsLost;

        public PlayerGameModel()
        {
        }

        public PlayerGameModel(PlayerModel pData)
        {
            Roads.CollectionChanged += Roads_CollectionChanged;
            Settlements.CollectionChanged += Settlements_CollectionChanged;
            Cities.CollectionChanged += Cities_CollectionChanged;
            Ships.CollectionChanged += Ships_CollectionChanged;
            PlayerModel = pData;
            PlayerTurnResourceCount = new PlayerResourceModel(pData);
            PlayerTurnResourceCount.OnPlayerResourceUpdate += OnGameModelResourceUpdate; // currently only logs that a resource was allocated
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        [JsonIgnore]
        public ObservableCollection<BuildingCtrl> Cities { get; } = new ObservableCollection<BuildingCtrl>();

        public int CitiesLeft => MaxCities - CitiesPlayed;

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

        public List<List<int>> GoldRolls
        {
            get
            {
                return _GoldRolls;
            }
            set
            {
                if (_GoldRolls != value)
                {
                    _GoldRolls = value;
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

        public bool LargestArmy
        {
            get => _LargestArmy;
            set
            {
                if (_LargestArmy != value)
                {
                    _LargestArmy = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("LongestRoadVisible");
                    UpdateScore();
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

        public Visibility LongestRoadVisible
        {
            get
            {
                return HasLongestRoad ? Visibility.Visible : Visibility.Collapsed;
            }
            set
            {
                HasLongestRoad = (value == Visibility.Visible) ? true : false;
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

        [JsonIgnore]
        public bool NotificationsEnabled { get; set; } = false;

        [JsonIgnore]
        public ObservableCollection<Harbor> OwnedHarbors { get; } = new ObservableCollection<Harbor>();

        public int Pips
        {
            get => pips;
            set
            {
                if (pips != value)
                {
                    //    12/14/2019: this line is causingt problems because this log gets added *before* the log for the settlment update,
                    //    which means Undo is broken because we stop undoing on a settlement changed event.  Undo always changes the collections,
                    //    so this gets updated anyway.  going to comment out to see if there are issues...

                    //LogPropertyChanged(pips, value); // this needs to be here so that the Pips are set when the log is replayed.  it'd be unfortunate if this was Undone...
                    pips = value;
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

        public PlayerModel PlayerModel
        {
            get
            {
                return _playerData;
            }
            set
            {
                _playerData = value;
                NotifyPropertyChanged();
            }
        }

        public PlayerResources Resources
        {
            get
            {
                return _resources;
            }
            set
            {
                if (_resources != value)
                {
                    _resources = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public PlayerResourceModel PlayerTurnResourceCount { get; set; } = null;

        [JsonIgnore]
        public ObservableCollection<RoadCtrl> Roads { get; private set; } = new ObservableCollection<RoadCtrl>();

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

        public int RoadsLeft => MaxRoads - RoadsPlayed;

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

        public TileOrientation RollOrientation
        {
            get
            {
                return _RollOrientation;
            }
            set
            {
                if (_RollOrientation != value)
                {
                    _RollOrientation = value;
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

        [JsonIgnore]
        public ObservableCollection<BuildingCtrl> Settlements { get; } = new ObservableCollection<BuildingCtrl>();

        public int SettlementsLeft => MaxSettlements - SettlementsPlayed;

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

        [JsonIgnore]
        public ObservableCollection<RoadCtrl> Ships { get; } = new ObservableCollection<RoadCtrl>();

        public int ShipsLeft => MaxShips - ShipsPlayed;

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

        public Visibility ShipsVisible
        {
            get
            {
                return (MaxShips > 0 ? Visibility.Visible : Visibility.Collapsed);
            }
        }

        public SyncronizedPlayerRolls SyncronizedPlayerRolls { get; } = new SyncronizedPlayerRolls();

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

        private void Cities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CitiesPlayed = Cities.Count;
            UpdateScore();
            Pips = CalculatePips(Settlements, Cities);
        }

        private void LogPropertyChanged(string oldVal, string newVal, bool stopUndo = false, [CallerMemberName] string propertyName = "")
        {
            //_playerData.Log?.PostLogEntry(_playerData, GameState.Unknown,
            //                                                 CatanAction.ChangedPlayerProperty, stopUndo, LogType.Normal, -1,
            //                                                 new LogPropertyChanged(propertyName, oldVal, newVal));
        }

        private void LogPropertyChanged(int oldVal, int newVal, bool stopUndo = false, [CallerMemberName] string propertyName = "")
        {
            LogPropertyChanged(oldVal.ToString(), newVal.ToString(), stopUndo, propertyName);
        }

        private void LogPropertyChanged(bool oldVal, bool newVal, bool stopUndo = false, [CallerMemberName] string propertyName = "")
        {
            LogPropertyChanged(oldVal.ToString(), newVal.ToString(), stopUndo, propertyName);
        }

        private void LogPropertyChanged(bool? oldVal, bool? newVal, bool stopUndo = false, [CallerMemberName] string propertyName = "")
        {
            LogPropertyChanged(oldVal.ToString(), newVal.ToString(), stopUndo, propertyName);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (!NotificationsEnabled) return; // this allows us to stop UI interactions during AddPlayer
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnGameModelResourceUpdate(PlayerModel player, ResourceType resource, int oldVal, int newVal)
        {
            // _playerData.Log.PostLogEntry(player, GameState.Unknown, CatanAction.AddResourceCount, false, LogType.Normal, newVal - oldVal, new LogResourceCount(oldVal, newVal, resource));
        }

        private void Roads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RoadsPlayed = Roads.Count();
        }

        private void Settlements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SettlementsPlayed = Settlements.Count;
            UpdateScore();
            Pips = CalculatePips(Settlements, Cities);
        }

        private void Ships_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ShipsPlayed = Ships.Count;
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

        public static string[] NonDisplayData()
        {
            return new string[] { "Roads", "Ships", "Buildings", "Rolls" };
        }

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

        // back pointer
        public void AddOwnedHarbor(Harbor harbor)
        {
            OwnedHarbors.Add(harbor);
        }

        public bool Deserialize(string s, bool oneLine)
        {
            StaticHelpers.DeserializeObject<PlayerGameModel>(this, s, ":", "|");
            return true;
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

        public void RemoveOwnedHarbor(Harbor harbor)
        {
            OwnedHarbors.Remove(harbor);
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
            PlayerTurnResourceCount.OnPlayerResourceUpdate -= OnGameModelResourceUpdate;
            PlayerTurnResourceCount.GameReset();
            Pips = 0;
            _GoldRolls = new List<List<int>>();

            for (int i = 0; i < _RoadTie.Count(); i++)
            {
                _RoadTie[i] = false;
            }
        }

        public string Serialize(bool oneLine)
        {
            return StaticHelpers.SerializeObject<PlayerGameModel>(this, _savedGameProperties, "=", "|");
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

        public void UpdateResourceCount(RollResourcesModel rrModel, LogState logState)
        {
            int mult = 1;
            if (logState == LogState.Undo) mult = -1;

            if (rrModel.BlockedByBaron)
            {
                CardsLostToBaron += (rrModel.Value * mult);
            }
            else
            {
                //  ResourcesAcquired += (rrModel.Value * mult);
                PlayerTurnResourceCount.AddResourceCount(rrModel.ResourceType, rrModel.Value * mult);
            }
        }

        public bool WinsRoadCountTie(int roadCount)
        {
            Debug.Assert(roadCount >= 5 && roadCount <= 15, "bad roadcount");
            return _RoadTie[roadCount - 5];
        }
    }

    /// <summary>
    ///     PlayerResourceData:  a class that keeps track of the number of resources that happen on a per roll basis.
    /// </summary>

    public class PlayerResourceModel : INotifyPropertyChanged
    {
        private readonly PlayerModel _player = null;

        private int _Brick = 0;
        private int _Gold = 0;
        private int _Ore = 0;
        private int _Sheep = 0;
        private int _Wheat = 0;
        private int _Wood = 0;
        public PlayerResourceUpdateHandler OnPlayerResourceUpdate;

        public PlayerResourceModel(PlayerModel player)
        {
            _player = player;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Brick
        {
            get => _Brick;
            private set
            {
                if (_Brick != value)
                {
                    _Brick = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }

        public int GoldMine
        {
            get => _Gold;
            private set
            {
                if (_Gold != value)
                {
                    _Gold = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }

        public int Ore
        {
            get => _Ore;
            private set
            {
                if (_Ore != value)
                {
                    _Ore = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }

        public int Sheep
        {
            get => _Sheep;
            private set
            {
                if (_Sheep != value)
                {
                    _Sheep = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }

        public int Total => Sheep + Wood + Ore + Wheat + Brick + GoldMine;

        public int Wheat
        {
            get => _Wheat;
            private set
            {
                if (_Wheat != value)
                {
                    _Wheat = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }

        public int Wood
        {
            get => _Wood;
            private set
            {
                if (_Wood != value)
                {
                    _Wood = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Total");
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
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
                    oldVal = GoldMine;
                    GoldMine += count;
                    break;

                default:
                    break;
            }

            OnPlayerResourceUpdate?.Invoke(_player, resource, oldVal, oldVal + count);

            return oldVal;
        }

        public void GameReset()
        {
            TurnReset();
        }

        public void TurnReset()
        {
            GoldMine = 0;
            Wheat = 0;
            Ore = 0;
            Sheep = 0;
            Brick = 0;
            Wood = 0;
        }
    }

    /// <summary>
    ///     this class has
    ///         1. the list of the rolls a player has made
    ///         2. the values of the 2 dice rolled
    ///     It also knows
    ///         1. How to compare the full list of Rolls while preserving order
    ///            (e.g. person 1 with rolls 5,7,7,4 wins over person 2 with 5,7,7,3 and person 3 with rolls 6 wins over all)
    ///         2. how to tell if to SynchronizedPlayerRolls are in a tie
    /// </summary>

    public class SyncronizedPlayerRolls : IComparable<SyncronizedPlayerRolls>, INotifyPropertyChanged
    {
        private int _DiceOne = -1;
        private int _DiceTwo = -1;

        public event PropertyChangedEventHandler PropertyChanged;

        public int DiceOne
        {
            get
            {
                return _DiceOne;
            }
            set
            {
                if (_DiceOne != value)
                {
                    _DiceOne = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("LatestRoll");
                    NotifyPropertyChanged("ShowLatestRoll");
                }
            }
        }

        public int DiceTwo
        {
            get
            {
                return _DiceTwo;
            }
            set
            {
                if (_DiceTwo != value)
                {
                    _DiceTwo = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("LatestRoll");
                    NotifyPropertyChanged("ShowLatestRoll");
                }
            }
        }

        public int Hash
        {
            get
            {
                int total = 0;
                foreach (var roll in Rolls)
                {
                    total = total * 10 + roll;
                }
                return total;
            }
        }

        public int LatestRoll => DiceOne + DiceTwo;
        public List<int> Rolls { get; set; } = new List<int>();

        public bool ShowLatestRoll
        {
            get
            {
                return ((DiceOne > 0 && DiceTwo > 0));
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddRoll(int d1, int d2)
        {
            DiceOne = d1;
            DiceTwo = d2;
            Rolls.Add(d1 + d2);
        }

        public int CompareTo(SyncronizedPlayerRolls other)
        {
            if (this.Rolls.Count == 0)
            {
                return this.Rolls.Count - other.Rolls.Count;
            }
            if (other.Rolls.Count == 0)
            {
                return -1;
            }
            int max = Math.Max(this.Rolls.Count, other.Rolls.Count);

            for (int i = 0; i < max; i++)
            {
                if (i < this.Rolls.Count && i < other.Rolls.Count)
                {
                    if (this.Rolls[i] == other.Rolls[i]) continue;   // tie

                    if (this.Rolls[i] < other.Rolls[i])
                    {
                        return 1; // b bigger
                    }
                    else
                    {
                        return -1; // b smaller
                    }
                }
            }

            if (this.Rolls.Count == other.Rolls.Count) return 0;  // tie for all rolls!
                                                                  //
                                                                  //   this means that there is a tie, but somebody has extra rolls -- call it a ties

            return 0;
        }

        public bool InTie(SyncronizedPlayerRolls roll)
        {
            Contract.Assert(roll != null);
            if (roll.Rolls.Count == 0) return true;

            foreach (var r in Rolls)
            {
                if (r.CompareTo(roll) == 0)
                {
                    if (roll.Rolls.Count <= this.Rolls.Count) // the same or missing a roll
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TiedWith(List<int> rolls)
        {
            if (Math.Abs(rolls.Count - Rolls.Count) > 1) return false; //

            int count = Math.Min(rolls.Count, Rolls.Count);
            for (int i = 0; i < count; i++)
            {
                if (rolls[i] != Rolls[i])
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}