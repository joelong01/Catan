using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catan10
{
    public class CatanGameData : INotifyPropertyChanged
    {
        public CatanGameData()
        {
           
        }


        #region properties

        private bool _allowShips = false;
        private RandomBoardSettings _boardSettings = new RandomBoardSettings();
        private List<DevCardType> _devCards = new List<DevCardType>();
        private CatanGames _gameName = CatanGames.Regular;
        private GameType _gameType = GameType.Regular;
        private int _harborCount = 9;
        private int _knight = 14;
        private int _maxCities = 4;
        private int _maxResourceAllocated = 19;
        private int _maxRoads = 15;
        private int _maxSettlements = 5;
        int _maxShips = 0;
        private int _monopoly = 2;
        private int _roadBuilding = 2;
        private int _tileCount = 19;
        private int _victoryPoint = 5;
        private int _yearOfPlenty = 2;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool AllowShips
        {
            get
            {
                return _allowShips;
            }
            set
            {
                if (value != _allowShips)
                {
                    _allowShips = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RandomBoardSettings BoardSettings
        {
            get
            {
                return _boardSettings;
            }
            set
            {
                if (value != _boardSettings)
                {
                    _boardSettings = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<DevCardType> DevCards
        {
            get
            {
                return _devCards;
            }
            set
            {
                if (value != _devCards)
                {
                    _devCards = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public CatanGames GameName
        {
            get
            {
                return _gameName;
            }
            set
            {
                if (value != _gameName)
                {
                    _gameName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public GameType GameType
        {
            get
            {
                return _gameType;
            }
            set
            {
                if (value != _gameType)
                {
                    _gameType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int HarborCount
        {
            get
            {
                return _harborCount;
            }
            set
            {
                if (value != _harborCount)
                {
                    _harborCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int Knight
        {
            get
            {
                return _knight;
            }
            set
            {
                if (value != _knight)
                {
                    _knight = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int MaxCities
        {
            get
            {
                return _maxCities;
            }
            set
            {
                if (value != _maxCities)
                {
                    _maxCities = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int MaxResourceAllocated
        {
            get
            {
                return _maxResourceAllocated;
            }
            set
            {
                if (value != _maxResourceAllocated)
                {
                    _maxResourceAllocated = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int MaxRoads
        {
            get
            {
                return _maxRoads;
            }
            set
            {
                if (value != _maxRoads)
                {
                    _maxRoads = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int MaxSettlements
        {
            get
            {
                return _maxSettlements;
            }
            set
            {
                if (value != _maxSettlements)
                {
                    _maxSettlements = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int MaxShips
        {
            get
            {
                return _maxShips;
            }
            set
            {
                if (value != _maxShips)
                {
                    _maxShips = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Monopoly
        {
            get
            {
                return _monopoly;
            }
            set
            {
                if (value != _monopoly)
                {
                    _monopoly = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int RoadBuilding
        {
            get
            {
                return _roadBuilding;
            }
            set
            {
                if (value != _roadBuilding)
                {
                    _roadBuilding = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int TileCount
        {
            get
            {
                return _tileCount;
            }
            set
            {
                if (value != _tileCount)
                {
                    _tileCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int VictoryPoint
        {
            get
            {
                return _victoryPoint;
            }
            set
            {
                if (value != _victoryPoint)
                {
                    _victoryPoint = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int YearOfPlenty
        {
            get
            {
                return _yearOfPlenty;
            }
            set
            {
                if (value != _yearOfPlenty)
                {
                    _yearOfPlenty = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion properties

        public CatanGameData(CatanGameData info)
        {
            MaxRoads = info.MaxRoads;
            MaxCities = info.MaxCities;
            MaxSettlements = info.MaxSettlements;
            MaxResourceAllocated = info.MaxResourceAllocated;
            AllowShips = info.AllowShips;
            Knight = info.Knight;
            VictoryPoint = info.VictoryPoint;
            YearOfPlenty = info.YearOfPlenty;
            RoadBuilding = info.RoadBuilding;
            Monopoly = info.Monopoly;
            BuildDevCardList();
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // for "Regular Game"
        // most aggregate resource per type
        public static bool operator !=(CatanGameData a, CatanGameData b)
        {
            return !(a == b);
        }

        // for "Regular Game"
        public static bool operator ==(CatanGameData a, CatanGameData b)
        {
            if (a is null)
            {
                if (b is null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (b is null) // a is not null!
            {
                return false;
            }
            return
            (
            a.MaxRoads == b.MaxRoads &&
            a.MaxCities == b.MaxCities &&
            a.MaxSettlements == b.MaxSettlements &&
            a.Knight == b.Knight &&
            a.MaxResourceAllocated == b.MaxResourceAllocated &&
            a.AllowShips == b.AllowShips &&
            a.VictoryPoint == b.VictoryPoint &&
            a.YearOfPlenty == b.YearOfPlenty &&
            a.RoadBuilding == b.RoadBuilding &&
            a.Monopoly == b.Monopoly
            );
        }

        public void BuildDevCardList()
        {
            DevCards.Clear();
            for (int i = 0; i < Knight; i++)
            {
                DevCards.Add(DevCardType.Knight);
            }
            for (int i = 0; i < VictoryPoint; i++)
            {
                DevCards.Add(DevCardType.VictoryPoint);
            }
            for (int i = 0; i < YearOfPlenty; i++)
            {
                DevCards.Add(DevCardType.YearOfPlenty);
            }
            for (int i = 0; i < Monopoly; i++)
            {
                DevCards.Add(DevCardType.Monopoly);
            }
            for (int i = 0; i < RoadBuilding; i++)
            {
                DevCards.Add(DevCardType.RoadBuilding);
            }
        }

        public override bool Equals(object obj)
        {
            return (CatanGameData)obj == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public DevCardType GetNextDevCard()
        {
            if (DevCards.Count == 0) return DevCardType.Unknown;
            Random rand = new Random((int)DateTime.Now.Ticks);
            int index = rand.Next(DevCards.Count);
            var devCard = DevCards[index];
            DevCards.RemoveAt(index);
            return devCard;
        }
    }
}