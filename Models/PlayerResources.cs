using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Catan10
{
    public class PlayerResources : INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private int _cities = 0;

        private TradeResources _currentResources = new TradeResources();

        private int _goldTotal = 0;

        private int _knightsPlayed = 0;

        private TradeResources _resourcesLostSeven = new TradeResources();

        private TradeResources _resourcesLostToBaron = new TradeResources();

        private TradeResources _resourcesLostToMonopoly = new TradeResources();

        private int _roads = 0;

        private int _settlements = 0;

        private ResourceType _stolenResource = ResourceType.None;
        private DevCardModel _thisTurnsDevCard = new DevCardModel() { DevCardType = DevCardType.None };

        private TradeResources _totalResources = new TradeResources();

     

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public ObservableCollection<DevCardModel> AvailableDevCards { get; set; } = new ObservableCollection<DevCardModel>();

        private int _victoryPoints = 0;
        public int VictoryPoints
        {
            get
            {
                return _victoryPoints;
            }
            set
            {
                if (value != _victoryPoints)
                {
                    _victoryPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     How many Cities the player has
        /// </summary>
        public int Cities
        {
            get
            {
                return _cities;
            }
            set
            {
                if (_cities != value)
                {
                    _cities = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The Resources the player has in their hand
        /// </summary>
        public TradeResources CurrentResources
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
                return _goldTotal;
            }
            set
            {
                if (_goldTotal != value)
                {
                    _goldTotal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Total number of knights played in the current game
        /// </summary>
        public int KnightsPlayed
        {
            get => _knightsPlayed;
            set
            {
                if (_knightsPlayed != value)
                {
                    _knightsPlayed = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     a list of dev cards the player has bought this turn
        /// </summary>
        public ObservableCollection<DevCardModel> NewDevCards { get; set; } = new ObservableCollection<DevCardModel>();

        /// <summary>
        ///     a list of all the dev cards the player has played in this game
        /// </summary>
        public ObservableCollection<DevCardModel> PlayedDevCards { get; set; } = new ObservableCollection<DevCardModel>();

        /// <summary>
        ///     The resources the player gave up when a 7 was rolled
        /// </summary>
        public TradeResources ResourcesLostSeven
        {
            get
            {
                return _resourcesLostSeven;
            }
            set
            {
                if (_resourcesLostSeven != value)
                {
                    _resourcesLostSeven = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     the resource the player gave up when a Baron was placed on them
        /// </summary>
        public TradeResources ResourcesLostToBaron
        {
            get
            {
                return _resourcesLostToBaron;
            }
            set
            {
                if (_resourcesLostToBaron != value)
                {
                    _resourcesLostToBaron = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     the resources lost when another player played monopolyz
        /// </summary>
        public TradeResources ResourcesLostToMonopoly
        {
            get
            {
                return _resourcesLostToMonopoly;
            }
            set
            {
                if (_resourcesLostToMonopoly != value)
                {
                    _resourcesLostToMonopoly = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The resources the player got this turn
        /// </summary>
        public ResourceCardCollection ResourcesThisTurn { get; } = new ResourceCardCollection(true);

        /// <summary>
        ///     How many roads the player has
        /// </summary>
        public int Roads
        {
            get
            {
                return _roads;
            }
            set
            {
                if (_roads != value)
                {
                    _roads = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     How many Settlements the player has
        /// </summary>
        public int Settlements
        {
            get
            {
                return _settlements;
            }
            set
            {
                if (_settlements != value)
                {
                    _settlements = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     How many StolenResources the player has
        /// </summary>
        public ResourceType StolenResource
        {
            get
            {
                return _stolenResource;
            }
            set
            {
                if (_stolenResource != value)
                {
                    _stolenResource = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The one dev card played this turn
        /// </summary>
        public DevCardModel ThisTurnsDevCard
        {
            get
            {
                return _thisTurnsDevCard;
            }
            set
            {
                if (_thisTurnsDevCard != value)
                {
                    _thisTurnsDevCard = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     the list of what resources the user has ever gotten.
        /// </summary>
        public ResourceCardCollection TotalResourcesCollection { get; set; } = new ResourceCardCollection(true);

        /// <summary>
        ///     the total number of resources the player has gotten by any means this game
        /// </summary>
        public TradeResources TotalResourcesForGame
        {
            get
            {
                return _totalResources;
            }
            set
            {
                if (_totalResources != value)
                {
                    _totalResources = value;
                    NotifyPropertyChanged();
                }
            }
        }
       

        public ObservableCollection<Entitlement> UnspentEntitlements { get; } = new ObservableCollection<Entitlement>();

        #endregion Properties

        #region Constructors + Destructors

        public PlayerResources()
        {
           
        }

        #endregion Constructors + Destructors

        #region Methods

        public void AddDevCard(DevCardType devCardType)
        {
            DevCardModel model = new DevCardModel() { DevCardType = devCardType };
            NewDevCards.Add(model);
        }
        public int GetUnspentEntitlements(Entitlement entitlement)
        {
            int count = 0;
            foreach (var e in UnspentEntitlements)
            {
                if (e == entitlement) count++;
            }
            return count;
        }
        public bool CanAfford(Entitlement entitlement)
        {
            TradeResources cost = TradeResources.GetEntitlementCost(entitlement);
            Contract.Assert(cost != null);
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                if (cost.CountForResource(resourceType) > this.CurrentResources.CountForResource(resourceType))
                    return false;
            }

            return true;
        }

        public bool Equivalent(TradeResources tradeResources)
        {
            this.CurrentResources.Equivalent(tradeResources);
            return true;
        }

        public void GrantEntitlement(Entitlement entitlement)
        {
            UnspentEntitlements.Add(entitlement);
        }

        /// <summary>
        ///     This will give and take resources away from a player.
        ///     Some trades (e.g. stealing a card with a BuyOrUpgradeKnight) are not publicly visible.
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="publiclyVisible"></param>
        public void GrantResources(TradeResources tr, bool publiclyVisible = true)
        {
            CurrentResources += tr;

            if (publiclyVisible)
            {
                ResourcesThisTurn.AddResources(tr);
            }
            if (tr.Count != 0)
            {
                TotalResourcesForGame += tr;
                MainPage.Current.MainPageModel.GameResources += tr;
                TotalResourcesCollection.AddResources(tr);
            }
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

        public bool HasUnusedEntitlment(Entitlement entitlement)
        {
            return HasEntitlement(entitlement);
        }

        public void MakeDevCardsAvailable()
        {
            NewDevCards.ForEach((dc) => AvailableDevCards.Add(dc));
            NewDevCards.Clear();
        }

        public bool PlayDevCard(DevCardType devCardType)
        {
            for (int i = AvailableDevCards.Count - 1; i >= 0; i--)
            {
                DevCardModel card = AvailableDevCards[i];
                if (card.DevCardType == devCardType)
                {
                    PlayedDevCards.Add(card);
                    AvailableDevCards.RemoveAt(i);
                    ThisTurnsDevCard = card;
                    card.Played = true;
                    return true;
                }
            }

            return false;
        }

        // the list of cards that have been played.  this is public information!
        public override string ToString()
        {
            return $"[Total={CurrentResources}][Ore={CurrentResources.Ore}][Brick={CurrentResources.Brick}][Wheat={CurrentResources.Wheat}][Wood={CurrentResources.Wood}][Sheep={CurrentResources.Sheep}] [DevCards={PlayedDevCards?.Count}][Stuff={Settlements + Roads + Cities}]";
        }

        /// <summary>
        ///     Move the DevCardPlayedThisTurn back and reset it
        /// </summary>
        /// <returns></returns>
        public bool UndoPlayDevCard(DevCardType type)
        {
            Contract.Assert(ThisTurnsDevCard.Played);
            Contract.Assert(type == ThisTurnsDevCard.DevCardType);
            ThisTurnsDevCard.Played = false;
            PlayedDevCards.Remove(ThisTurnsDevCard);
            AvailableDevCards.Add(ThisTurnsDevCard);
            ThisTurnsDevCard = new DevCardModel() { DevCardType = DevCardType.None }; ;
            return true;
        }

        internal bool ConsumeEntitlement(Entitlement entitlement)
        {
            Debug.Assert(HasEntitlement(entitlement));
            if (HasEntitlement(entitlement))
            {
                UnspentEntitlements.Remove(entitlement);
                return true;
            }
            return false;
        }

        internal void RevokeEntitlement(Entitlement entitlement)
        {
            this.ConsumeEntitlement(entitlement);
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}