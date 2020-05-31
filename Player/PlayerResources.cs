using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Catan10
{
    public class PlayerResources : INotifyPropertyChanged
    {
        private int _Cities = 0;

        private TradeResources _currentResources = new TradeResources();
        private int _GoldTotal = 0;
        private int _PlayedMonopoly = 0;
        private int _PlayedRoadBuilding = 0;
        private int _PlayedYearOfPlenty = 0;
        private TradeResources _ResourcesLostToBard = new TradeResources();
        private TradeResources _ResourcesThisTurn = new TradeResources();
        private int _Roads = 0;
        private int _Settlements = 0;
        private int _TotalDevCards = 0;
        private TradeResources _TotalResources = new TradeResources();
        private int _UnplayedKnights = 0;
        private int _UnplayedMonopoly = 0;
        private int _UnplayedYearOfPlenty = 0;
        private int _VictoryPoints = 0;
        TradeResources _ResourcesLostToMonopoly = new TradeResources();
        TradeResources _ResourcesLostSeven = new TradeResources();
        public TradeResources ResourcesLostSeven
        {
            get
            {
                return _ResourcesLostSeven;
            }
            set
            {
                if (_ResourcesLostSeven != value)
                {
                    _ResourcesLostSeven = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CanAfford(Entitlement entitlement)
        {
            TradeResources cost = TradeResources.GetEntitlementCost(entitlement);
            Contract.Assert(cost != null);
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                if (cost.CountForResource(resourceType) > this.Current.CountForResource(resourceType))
                    return false;
            }

            return true;
        }

        public TradeResources ResourcesLostToMonopoly
        {
            get
            {
                return _ResourcesLostToMonopoly;
            }
            set
            {
                if (_ResourcesLostToMonopoly != value)
                {
                    _ResourcesLostToMonopoly = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public PlayerResources()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Cities
        {
            get
            {
                return _Cities;
            }
            set
            {
                if (_Cities != value)
                {
                    _Cities = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TradeResources Current
        {
            get
            {
                return _currentResources;
            }
            set
            {
                if (_currentResources != value)
                {
                    _currentResources = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int GoldTotal
        {
            get
            {
                return _GoldTotal;
            }
            set
            {
                if (_GoldTotal != value)
                {
                    _GoldTotal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<DevCardType> PlayedDevCards { get; set; } = new List<DevCardType>();

        public int PlayedMonopoly
        {
            get
            {
                return _PlayedMonopoly;
            }
            set
            {
                if (_PlayedMonopoly != value)
                {
                    _PlayedMonopoly = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int PlayedRoadBuilding
        {
            get
            {
                return _PlayedRoadBuilding;
            }
            set
            {
                if (_PlayedRoadBuilding != value)
                {
                    _PlayedRoadBuilding = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int PlayedYearOfPlenty
        {
            get
            {
                return _PlayedYearOfPlenty;
            }
            set
            {
                if (_PlayedYearOfPlenty != value)
                {
                    _PlayedYearOfPlenty = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TradeResources ResourcesLostToBaron
        {
            get
            {
                return _ResourcesLostToBard;
            }
            set
            {
                if (_ResourcesLostToBard != value)
                {
                    _ResourcesLostToBard = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TradeResources ResourcesThisTurn
        {
            get
            {
                return _ResourcesThisTurn;
            }
            set
            {
                if (_ResourcesThisTurn != value)
                {
                    _ResourcesThisTurn = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int Roads
        {
            get
            {
                return _Roads;
            }
            set
            {
                if (_Roads != value)
                {
                    _Roads = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int Settlements
        {
            get
            {
                return _Settlements;
            }
            set
            {
                if (_Settlements != value)
                {
                    _Settlements = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int TotalDevCards
        {
            get
            {
                return _TotalDevCards;
            }
            set
            {
                if (_TotalDevCards != value)
                {
                    _TotalDevCards = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TradeResources TotalResources
        {
            get
            {
                return _TotalResources;
            }
            set
            {
                if (_TotalResources != value)
                {
                    _TotalResources = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public int UnplayedDevCards => UnplayedKnights + VictoryPoints + UnplayedYearOfPlenty + UnplayedRoadBuilding + UnplayedMonopoly;

        public int UnplayedKnights
        {
            get
            {
                return _UnplayedKnights;
            }
            set
            {
                if (_UnplayedKnights != value)
                {
                    _UnplayedKnights = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int UnplayedMonopoly
        {
            get
            {
                return _UnplayedMonopoly;
            }
            set
            {
                if (_UnplayedMonopoly != value)
                {
                    _UnplayedMonopoly = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int UnplayedRoadBuilding { get; set; } = 0;

        public int UnplayedYearOfPlenty
        {
            get
            {
                return _UnplayedYearOfPlenty;
            }
            set
            {
                if (_UnplayedYearOfPlenty != value)
                {
                    _UnplayedYearOfPlenty = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<Entitlement> UnspentEntitlements { get; } = new ObservableCollection<Entitlement>();

        public int VictoryPoints
        {
            get
            {
                return _VictoryPoints;
            }
            set
            {
                if (_VictoryPoints != value)
                {
                    _VictoryPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void ConsumeEntitlement(Entitlement entitlement)
        {
            Contract.Assert(HasEntitlement(entitlement));
            UnspentEntitlements.Remove(entitlement);
        }

        internal void RevokeEntitlement(Entitlement entitlement)
        {
            this.ConsumeEntitlement(entitlement);
        }

        public bool Equivalent(TradeResources tradeResources)
        {
            this.Current.Equivalent(tradeResources);
            return true;
        }

        public void GrantEntitlement(Entitlement entitlement)
        {
            UnspentEntitlements.Add(entitlement);
        }

        

        public void GrantResources(TradeResources tr)
        {
            Current += tr;
            ResourcesThisTurn += tr;
            TotalResources += tr;
            NotifyPropertyChanged("PlayerResources");
            NotifyPropertyChanged("Current");
            NotifyPropertyChanged("ResourcesThisTurn");
            NotifyPropertyChanged("TotalResources");
            NotifyPropertyChanged("EnabledEntitlementPurchase");

        }

        public bool HasEntitlement(Entitlement entitlement)
        {
            return UnspentEntitlements.Contains(entitlement);
        }

        // the list of cards that have been played.  this is public information!
        public override string ToString()
        {
            return $"[Total={Current}][Ore={Current.Ore}][Brick={Current.Brick}][Wheat={Current.Wheat}][Wood={Current.Wood}][Sheep={Current.Sheep}] [DevCards={PlayedDevCards?.Count}][Stuff={Settlements + Roads + Cities}]";
        }
    }
}