﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

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
        public ObservableCollection<int> Rolls { get; set; } = new ObservableCollection<int>();
        public PlayerResourceData PlayerResourceData { get; set; } = null;
        private List<string> _savedGameProperties = new List<string> { "PlayerIdentified", "Score", "ResourceCount", "KnightsPlayed","TimesTargeted", "NoResourceCount", "RollsWithResource", "MaxNoResourceRolls", "CardsLost", "CardsLostToSeven", "CardsLostToMonopoly", "ResourcesAcquired",
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
            foreach (var s in Settlements)
            {
                foreach (var kvp in s.BuildingToTileDictionary)
                {
                    pips += kvp.Value.Pips;
                }
            }
            foreach (var s in Cities)
            {
                foreach (var kvp in s.BuildingToTileDictionary)
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
            //
            //  sigh.  when you make it easy to set MaxNoResourceRolls, you make it hard to
            //         make the value less.  I'll just go around the property setter to make 
            //         it zero, and update the UI
            _maxNoResourceRolls = 0;
            NotifyPropertyChanged("MaxNoResourceRolls");
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
            Rolls.Clear();
            Ships.Clear();
            IsCurrentPlayer = false;
            MaxShips = 0;
            MaxRoads = 0;
            MaxSettlements = 0;
            MaxCities = 0;
            PlayerResourceData.Reset();
            Pips = 0;

            for (int i=0; i< _RoadTie.Count(); i++)
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
            Debug.Assert (roadCount >= 5 && roadCount <= 15, "bad roadcount");
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

        public int CitiesLeft
        {
            get
            {
                return MaxCities - CitiesPlayed;
            }
        }

        public int SettlementsLeft
        {
            get
            {
                return MaxSettlements - SettlementsPlayed;
            }
        }

        public int RoadsLeft
        {
            get
            {
                return MaxRoads - RoadsPlayed;
            }
        }

        public int ShipsLeft
        {
            get
            {
                return MaxShips - ShipsPlayed;
            }
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
            get
            {
                return _useLightFile;
            }
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
            get
            {
                return _ColorAsString;
            }
            set
            {
                if (_ColorAsString != value)
                {
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
            get
            {
                return pips;
            }
            set
            {
                if (pips != value)
                {
                    pips = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int MaxSettlements
        {
            get
            {
                return _MaxSettlements;
            }
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
            get
            {
                return _MaxCities;
            }
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
            get
            {
                return _MaxRoads;
            }
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
            get
            {
                return _MaxShips;
            }
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
            get
            {
                return _isCurrentPlayer;
            }
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
            get
            {
                return _goodRoll;
            }
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
            get
            {
                return _IslandsPlayed;
            }
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
            get
            {
                return _CardsLostToBaron;
            }
            set
            {
                if (_CardsLostToBaron != value)
                {
                    _CardsLostToBaron = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Guid PlayerIdentifier
        {
            get
            {
                return _PlayerIdentifier;
            }
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
            get
            {
                return _PlayedKnightThisTurn;
            }
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
            get
            {
                return _MovedBaronAfterRollingSeven;
            }
            set
            {
                if (_MovedBaronAfterRollingSeven != value)
                {
                    _MovedBaronAfterRollingSeven = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public TimeSpan TotalTime
        {
            get
            {
                return _TotalTime;
            }
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
            get
            {
                return _SettlementsPlayed;
            }
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
            get
            {
                return _CitiesPlayed;
            }
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
            get
            {
                return _RoadsPlayed;
            }
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
            get
            {
                return _ShipsPlayed;
            }
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
            get
            {
                return _LongestRoad;
            }
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
            get
            {
                return _HasLongestRoad;
            }
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
            get
            {
                return _LargestArmy;
            }
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
            get
            {
                return _ResourcesAcquired;
            }
            set
            {
                if (_ResourcesAcquired != value)
                {
                    _ResourcesAcquired = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int CardsLostToSeven
        {
            get
            {
                return _CardsLostToSeven;
            }
            set
            {
                if (_CardsLostToSeven != value)
                {
                    _CardsLostToSeven = value;
                    NotifyPropertyChanged();
                }
            }
        }

        int _CardsLostToMonopoly = 0;
        public int CardsLostToMonopoly
        {
            get
            {
                return _CardsLostToMonopoly;
            }
            set
            {
                if (_CardsLostToMonopoly != value)
                {
                    _CardsLostToMonopoly = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int CardsLost
        {
            get
            {
                return _cardsLost;
            }
            set
            {
                if (_cardsLost != value)
                {
                    OnCardsLost?.Invoke(_playerData, _cardsLost, value);
                    _cardsLost = value;
                    NotifyPropertyChanged();
                }
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
                if (_timesTargeted != value)
                {

                    _timesTargeted = value;
                    NotifyPropertyChanged();
                }
            }
        }



        public int NoResourceCount
        {
            get
            {
                return _noResourceCount;
            }
            set
            {
                if (_noResourceCount != value)
                {

                    _noResourceCount = value;
                    MaxNoResourceRolls = value; // only takes on Set
                    NotifyPropertyChanged();
                }
            }
        }
        public int RollsWithResource
        {
            get
            {
                return _rollsWithResource;
            }
            set
            {
                if (_rollsWithResource != value)
                {

                    _rollsWithResource = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int MaxNoResourceRolls
        {
            get
            {
                return _maxNoResourceRolls;
            }
            set
            {
                if (_maxNoResourceRolls < value) // only record the max
                {

                    _maxNoResourceRolls = value;
                    //
                    //  TODO:  log this value
                    NotifyPropertyChanged();
                }
            }
        }

        public int KnightsPlayed
        {
            get
            {
                return _knightsPlayed;
            }
            set
            {
                if (_knightsPlayed != value)
                {
                    _knightsPlayed = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Score
        {
            get
            {
                return _score;
            }
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
        // returns the number of resources the user actually got
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
                value = 1;
            else if (buildingState == BuildingState.City)
                value = 2;

            if (undo) value = -value;

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
        private PlayerData _playerData = null;
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
            get
            {
                return _Gold;
            }
            set
            {
                OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.GoldMine, value);
                if (_Gold != value)
                {
                    _Gold = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Wheat
        {
            get
            {
                return _Wheat;
            }
            set
            {
                OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.Wheat, value);
                if (_Wheat != value)
                {
                    _Wheat = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Brick
        {
            get
            {
                return _Brick;
            }
            set
            {
                OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.Brick, value);
                if (_Brick != value)
                {
                    _Brick = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Ore
        {
            get
            {
                return _Ore;
            }
            set
            {
                OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.Ore, value);
                if (_Ore != value)
                {
                    _Ore = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Wood
        {
            get
            {
                return _Wood;
            }
            set
            {
                OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.Wood, value);
                if (_Wood != value)
                {
                    _Wood = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Sheep
        {
            get
            {
                return _Sheep;
            }
            set
            {
                OnPlayerResourceUpdate?.Invoke(_playerData, ResourceType.Sheep, value);
                if (_Sheep != value)
                {
                    _Sheep = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        internal void AddResourceCount(ResourceType resource, int count)
        {
            switch (resource)
            {
                case ResourceType.Sheep:
                    Sheep += count;
                    break;
                case ResourceType.Wood:
                    Wood += count;
                    break;
                case ResourceType.Ore:
                    Ore += count;
                    break;
                case ResourceType.Wheat:
                    Wheat += count;
                    break;
                case ResourceType.Brick:
                    Brick += count;
                    break;
                case ResourceType.GoldMine:
                    Gold += count;
                    break;
                default:
                    break;
            }
        }
    }
}