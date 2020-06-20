﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

using Catan.Proxy;

using Windows.UI.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public class PlayerGameModel : INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        public CardsLostUpdatedHandler OnCardsLost;
        private readonly bool[] _RoadTie = new bool[10];

        // does this instance win the ties for this count of roads?
        private int _CitiesPlayed = 0;

        private int _goldRolls = 0;
        private bool _goodRoll = false;
        private bool _HasLongestRoad = false;
        private bool _isCurrentPlayer = false;
        private Dictionary<Island, int> _islands = new Dictionary<Island, int>();
        private int _IslandsPlayed = 0;
        private bool _LargestArmy = false;
        private int _LongestRoad = 0;
        private int _MaxCities = 0;
        private int _maxNoResourceRolls = 0;
        private int _MaxRoads = 0;
        private int _MaxSettlements = 0;
        private int _MaxShips = 0;
        private bool? _MovedBaronAfterRollingSeven = null;
        private int _noResourceCount = 0;
        private int _pips = 0;
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
        Trades _trades = new Trades();

        #endregion Delegates + Fields + Events + Enums

        #region Properties

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

        public int GoldRolls
        {
            get
            {
                return _goldRolls;
            }
            set
            {
                if (_goldRolls != value)
                {
                    _goldRolls = value;
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
            get => _pips;
            set
            {
                if (_pips != value)
                {
                    _pips = value;
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

        public Trades Trades
        {
            get
            {
                return _trades;
            }
            set
            {
                if (_trades != value)
                {
                    _trades = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion Properties

        #region Constructors + Destructors

        public PlayerGameModel()
        {
            NotificationsEnabled = true;
        }

        public PlayerGameModel(PlayerModel pData)
        {
            Roads.CollectionChanged += Roads_CollectionChanged;
            Settlements.CollectionChanged += Settlements_CollectionChanged;
            Cities.CollectionChanged += Cities_CollectionChanged;
            Ships.CollectionChanged += Ships_CollectionChanged;
            PlayerModel = pData;
        }

        #endregion Constructors + Destructors

        #region Methods

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

            LongestRoad = 0;
            TimesTargeted = 0;
            NoResourceCount = 0;
            RollsWithResource = 0;
            MaxNoResourceRolls = 0;
            GoodRoll = false;
            Resources = new PlayerResources();
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

            Pips = 0;
            _goldRolls = 0;

            for (int i = 0; i < _RoadTie.Count(); i++)
            {
                _RoadTie[i] = false;
            }
        }

        public string Serialize(bool indented)
        {
            return CatanProxy.Serialize(this, indented);
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
            // if (!NotificationsEnabled) return; // this allows us to stop UI interactions during AddPlayer
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

        #endregion Methods
    }

    public class Trades : INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<TradeOffer> _potentialTrades = new ObservableCollection<TradeOffer>();

        TradeOffer _tradeRequest = new TradeOffer();

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public ObservableCollection<TradeOffer> PotentialTrades
        {
            get
            {
                return _potentialTrades;
            }
            set
            {
                if (_potentialTrades != value)
                {
                    _potentialTrades = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TradeOffer TradeRequest
        {
            get
            {
                return _tradeRequest;
            }
            set
            {
                if (_tradeRequest != value)
                {
                    _tradeRequest = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion Properties

        #region Methods

        public void OfferAccepted()
        {
            TradeRequest.OwnerApproved = false;
            PotentialTrades.ForEach((o) => o.OwnerApproved = false);
        }
    
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}
