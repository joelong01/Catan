using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Catan10
{
    public class TradeResources : INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private int _brick = 0;

        private int _goldMine = 0;

        private int _ore = 0;

        private int _sheep = 0;

        private int _wheat = 0;

        private int _wood = 0;

        private int _paper = 0;

        private int _cloth = 0;

        private int _coin = 0;


        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public int Brick
        {
            get
            {
                return _brick;
            }
            set
            {
                if (value != _brick)
                {
                    _brick = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }
        [JsonIgnore]
        public int Count => Wheat + Wood + Brick + Ore + Sheep + GoldMine;

        public int GoldMine
        {
            get
            {
                return _goldMine;
            }
            set
            {
                if (value != _goldMine)
                {
                    _goldMine = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }

        [JsonIgnore]
        public List<ResourceType> NonZeroResources
        {
            get
            {
                List<ResourceType> list = new List<ResourceType>();
                if (Brick != 0) list.Add(ResourceType.Brick);
                if (Wood != 0) list.Add(ResourceType.Wood);
                if (Wheat != 0) list.Add(ResourceType.Wheat);
                if (Sheep != 0) list.Add(ResourceType.Sheep);
                if (Ore != 0) list.Add(ResourceType.Ore);
                if (GoldMine != 0) list.Add(ResourceType.GoldMine);
                if (Coin != 0) list.Add(ResourceType.Coin);
                if (Cloth != 0) list.Add(ResourceType.Cloth);
                if (Paper != 0) list.Add(ResourceType.Paper);
                return list;
            }
        }

        public int Ore
        {
            get
            {
                return _ore;
            }
            set
            {
                if (value != _ore)
                {
                    _ore = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }

        public int Sheep
        {
            get
            {
                return _sheep;
            }
            set
            {
                if (value != _sheep)
                {
                    _sheep = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }

        public int Wheat
        {
            get
            {
                return _wheat;
            }
            set
            {
                if (value != _wheat)
                {
                    _wheat = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }

        public int Wood
        {
            get
            {
                return _wood;
            }
            set
            {
                if (value != _wood)
                {
                    _wood = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }
        public int Coin
        {
            get
            {
                return _coin;
            }
            set
            {
                if (value != _coin)
                {
                    _coin = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }
        public int Paper
        {
            get
            {
                return _paper;
            }
            set
            {
                if (value != _paper)
                {
                    _paper = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }
        public int Cloth
        {
            get
            {
                return _cloth;
            }
            set
            {
                if (value != _cloth)
                {
                    _cloth = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }
        #endregion Properties

        #region Constructors + Destructors

        public TradeResources()
        {
        }

        public TradeResources(TradeResources tradeResources)
        {
            Wheat = tradeResources.Wheat;
            Wood = tradeResources.Wood;
            Brick = tradeResources.Brick;
            Ore = tradeResources.Ore;
            Sheep = tradeResources.Sheep;
            GoldMine = tradeResources.GoldMine;
            Cloth = tradeResources.Cloth;
            Coin = tradeResources.Coin;
            Paper = tradeResources.Paper;
        }

        #endregion Constructors + Destructors

        #region Methods

        public static TradeResources GetEntitlementCost(Entitlement entitlement)
        {
            TradeResources cost = null;
            switch (entitlement)
            {
                case Entitlement.DevCard:
                    cost = new TradeResources()
                    {
                        Sheep = 1,
                        Wheat = 1,
                        Ore = 1
                    };
                    break;

                case Entitlement.Settlement:
                    cost = new TradeResources()
                    {
                        Sheep = 1,
                        Wheat = 1,
                        Wood = 1,
                        Brick = 1
                    };
                    break;

                case Entitlement.City:
                    cost = new TradeResources()
                    {
                        Wheat = 2,
                        Ore = 3
                    };
                    break;

                case Entitlement.Road:
                    cost = new TradeResources()
                    {
                        Wood = 1,
                        Brick = 1,
                    };
                    break;

                case Entitlement.Ship:
                    cost = new TradeResources()
                    {
                        Sheep = 1,
                        Wood = 1,
                    };
                    break;

                case Entitlement.BuyOrUpgradeKnight:
                    cost = new TradeResources()
                    {
                        Ore = 1,
                        Sheep = 1,
                    };
                    break;
                case Entitlement.ActivateKnight:
                    cost = new TradeResources()
                    {
                        Wheat = 1
                    };
                    break;
                case Entitlement.MoveKnight:
                    cost = new TradeResources()
                    {

                    };
                    break;
                case Entitlement.PoliticsUpgradeOne:
                    cost = new TradeResources()
                    {
                        Coin = 1
                    };
                    break;
                case Entitlement.PoliticsUpgradeTwo:
                    cost = new TradeResources()
                    {
                        Coin = 2
                    };
                    break;
                case Entitlement.PoliticsUpgradeThree:
                    cost = new TradeResources()
                    {
                        Coin = 3
                    };
                    break;
                case Entitlement.PoliticsUpgradeFour:
                    cost = new TradeResources()
                    {
                        Coin = 4
                    };
                    break;
                case Entitlement.PoliticsUpgradeFive:
                    cost = new TradeResources()
                    {
                        Coin = 5
                    };
                    break;
                case Entitlement.TradeUpgradeOne:
                    cost = new TradeResources()
                    {
                        Cloth = 1
                    };
                    break;
                case Entitlement.TradeUpgradeTwo:
                    cost = new TradeResources()
                    {
                        Cloth = 2
                    };
                    break;
                case Entitlement.TradeUpgradeThree:
                    cost = new TradeResources()
                    {
                        Cloth = 3
                    };
                    break;
                case Entitlement.TradeUpgradeFour:
                    cost = new TradeResources()
                    {
                        Cloth = 4
                    };
                    break;
                case Entitlement.TradeUpgradeFive:
                    cost = new TradeResources()
                    {
                        Cloth = 5
                    };
                    break;
                case Entitlement.ScienceUpgradeOne:
                    cost = new TradeResources()
                    {
                        Paper = 1
                    };
                    break;
                case Entitlement.ScienceUpgradeTwo:
                    cost = new TradeResources()
                    {
                        Paper = 2
                    };
                    break;
                case Entitlement.ScienceUpgradeThree:
                    cost = new TradeResources()
                    {
                        Paper = 3
                    };
                    break;
                case Entitlement.ScienceUpgradeFour:
                    cost = new TradeResources()
                    {
                        Paper = 4
                    };
                    break;
                case Entitlement.ScienceUpgradeFive:
                    cost = new TradeResources()
                    {
                        Paper = 5
                    };
                    break;
                case Entitlement.Undefined:
                default:
                    Contract.Assert(false, "Bad Entitlement");
                    break;
            }

            return cost;
        }

        public static bool GrantableResources(ResourceType resType)
        {
            switch (resType)
            {
                case ResourceType.Sheep:
                case ResourceType.Wood:
                case ResourceType.Ore:
                case ResourceType.Wheat:
                case ResourceType.Brick:
                case ResourceType.GoldMine:
                case ResourceType.Cloth:
                case ResourceType.Paper:
                case ResourceType.Coin:
                    return true;

                case ResourceType.Desert:
                case ResourceType.Back:
                case ResourceType.None:
                case ResourceType.Sea:
                default:
                    return false;
            }
        }

        //
        //  useful for the Resource Tests
        public static TradeResources operator +(TradeResources a, TradeResources b)
        {
            return new TradeResources()
            {
                Wheat = a.Wheat + b.Wheat,
                Wood = a.Wood + b.Wood,
                Ore = a.Ore + b.Ore,
                Sheep = a.Sheep + b.Sheep,
                Brick = a.Brick + b.Brick,
                GoldMine = a.GoldMine + b.GoldMine,
                Coin = a.Coin + b.Coin,
                Paper = a.Paper + b.Paper,
                Cloth = a.Cloth + b.Cloth

            };
        }

        public static TradeResources TradeResourcesForCity(ResourceType resourceType, bool pirates)
        {
            TradeResources tr = new TradeResources();

            switch (resourceType)
            {
                case ResourceType.Sheep:
                    tr.Sheep++;
                    tr.Cloth += pirates ? 1 : 0;
                    tr.Sheep += pirates ? 0 : 1;
                    break;
                case ResourceType.Wood:
                    tr.Wood++;
                    tr.Wood += pirates ? 0 : 1;
                    tr.Paper += pirates ? 1 : 0;
                    break;
                case ResourceType.Ore:
                    tr.Ore++;
                    tr.Ore += pirates ? 0 : 1;
                    tr.Coin += pirates ? 1 : 0;
                    break;
                case ResourceType.Wheat:
                    tr.Wheat += 2;
                    break;
                case ResourceType.Brick:
                    tr.Brick += 2;
                    break;
                case ResourceType.GoldMine:
                    tr.GoldMine += 2;
                    break;
                default:
                    break;
            }
            return tr;
        }

        public static TradeResources TradeResourcesForBuilding(BuildingState buildingState, ResourceType resourceType, bool pirates)
        {
            var tr = new TradeResources();
            switch (buildingState)
            {
                case BuildingState.Settlement:
                    tr.AddResource(resourceType, 1);
                    break;
                case BuildingState.City:
                case BuildingState.Metropolis:
                    return TradeResources.TradeResourcesForCity(resourceType, pirates);
            }
       
            return tr;
        }

        public void AddResource(ResourceType resourceType, int toAdd)
        {
            switch (resourceType)
            {
                case ResourceType.Sheep:
                    Sheep += toAdd;
                    break;

                case ResourceType.Wood:
                    Wood += toAdd;
                    break;

                case ResourceType.Ore:
                    Ore += toAdd;
                    break;

                case ResourceType.Wheat:
                    Wheat += toAdd;
                    break;

                case ResourceType.Brick:
                    Brick += toAdd;
                    break;

                case ResourceType.GoldMine:
                    GoldMine += toAdd;
                    break;
                case ResourceType.Cloth:
                    Cloth += toAdd;
                    break;
                case ResourceType.Coin:
                    Coin += toAdd;
                    break;
                case ResourceType.Paper:
                    Paper += toAdd;
                    break;
                case ResourceType.Desert:
                    break;

                case ResourceType.Back:

                case ResourceType.None:

                case ResourceType.Sea:

                default:
                    this.TraceMessage($"{resourceType} passed to Add()");
                    break;
            }
        }

        public bool CanAfford(TradeResources cost)
        {
            Contract.Assert(cost != null);
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                if (cost.CountForResource(resourceType) > this.CountForResource(resourceType))
                    return false;
            }

            return true;
        }

        public bool CanAfford(Entitlement entitlement)
        {
            TradeResources cost = TradeResources.GetEntitlementCost(entitlement);
            Contract.Assert(cost != null);
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                if (cost.CountForResource(resourceType) > this.CountForResource(resourceType))
                    return false;
            }

            return true;
        }

        public int CountForResource(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.Sheep:
                    return Sheep;

                case ResourceType.Wood:
                    return Wood;

                case ResourceType.Ore:
                    return Ore;

                case ResourceType.Wheat:
                    return Wheat;

                case ResourceType.Brick:
                    return Brick;

                case ResourceType.GoldMine:
                    return GoldMine;
                case ResourceType.Cloth:
                    return Cloth;
                case ResourceType.Paper:
                    return Paper;
                case ResourceType.Coin:
                    return Coin;

                case ResourceType.Desert:
                case ResourceType.Back:
                case ResourceType.None:
                case ResourceType.Sea:
                default:
                    return 0;
            }
        }

        public bool EqualValue(TradeResources b)
        {
            if (b == null) return false;
            return (
                ( this.Wheat == b.Wheat ) &&
                ( this.Wood == b.Wood ) &&
                ( this.Ore == b.Ore ) &&
                ( this.Sheep == b.Sheep ) &&
                ( this.Brick == b.Brick ) &&
                ( this.GoldMine == b.GoldMine ) &&
                ( this.Cloth == b.Cloth ) &&
                ( this.Coin == b.Coin ) &&
                ( this.Paper == b.Paper ) );
        }

        public bool Equivalent(TradeResources tradeResources)
        {
            if (Wheat != tradeResources.Wheat || Wood != tradeResources.Wood || Ore != tradeResources.Ore ||
                Sheep != tradeResources.Sheep || Brick != tradeResources.Brick || GoldMine != tradeResources.GoldMine ||
                Coin != tradeResources.Coin || Paper != tradeResources.Paper || Cloth != tradeResources.Cloth)
            {
                return false;
            }

            return true;
        }

        public TradeResources GetNegated()
        {
            return new TradeResources()
            {
                Wheat = -Wheat,
                Wood = -Wood,
                Ore = -Ore,
                Sheep = -Sheep,
                Brick = -Brick,
                GoldMine = -GoldMine,
                Cloth = -Cloth,
                Coin = -Coin,
                Paper = -Paper,
            };
        }

        public void SetResource(ResourceType resourceType, int value)
        {
            switch (resourceType)
            {
                case ResourceType.Sheep:
                    Sheep = value;
                    break;

                case ResourceType.Wood:
                    Wood = value;
                    break;

                case ResourceType.Ore:
                    Ore = value;
                    break;

                case ResourceType.Wheat:
                    Wheat = value;
                    break;

                case ResourceType.Brick:
                    Brick = value;
                    break;

                case ResourceType.GoldMine:
                    GoldMine = value;
                    break;
                case ResourceType.Cloth:
                    Cloth = value;
                    break;
                case ResourceType.Coin:
                    Coin = value;
                    break;
                case ResourceType.Paper:
                    Paper = value;
                    break;
                case ResourceType.Desert:
                    break;
                case ResourceType.Back:
                case ResourceType.None:
                case ResourceType.Sea:
                default:
                    this.TraceMessage($"{resourceType} passed to SetResource");
                    break;
            }
        }

        public List<ResourceType> ToList()
        {
            List<ResourceType> list = new List<ResourceType>();
            for (int i = 0; i < Brick; i++)
            {
                list.Add(ResourceType.Brick);
            }
            for (int i = 0; i < Wood; i++)
            {
                list.Add(ResourceType.Wood);
            }
            for (int i = 0; i < Wheat; i++)
            {
                list.Add(ResourceType.Wheat);
            }
            for (int i = 0; i < Ore; i++)
            {
                list.Add(ResourceType.Ore);
            }
            for (int i = 0; i < Sheep; i++)
            {
                list.Add(ResourceType.Sheep);
            }
            for (int i = 0; i < Coin; i++)
            {
                list.Add(ResourceType.Coin);
            }
            for (int i = 0; i < Paper; i++)
            {
                list.Add(ResourceType.Paper);
            }
            for (int i = 0; i < Cloth; i++)
            {
                list.Add(ResourceType.Cloth);
            }
            return list;
        }

        public ResourceCardCollection ToResourceCardCollection()
        {
            ResourceCardCollection list = new ResourceCardCollection();
            list.AddResources(this);
            return list;
        }

        public override string ToString()
        {
            return $"[Count={Count}][Ore={Ore}][Brick={Brick}][Wheat={Wheat}][Wood={Wood}][Sheep={Sheep}][Gold={GoldMine}][Coin={Coin}][Cloth={Cloth}][Paper={Paper}]";
        }

        internal int GetCount(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.Sheep:
                    return Sheep;

                case ResourceType.Wood:
                    return Wood;

                case ResourceType.Ore:
                    return Ore;

                case ResourceType.Wheat:
                    return Wheat;

                case ResourceType.Brick:
                    return Brick;

                case ResourceType.GoldMine:
                    return GoldMine;
                case ResourceType.Coin:
                    return Coin;
                case ResourceType.Paper:
                    return Paper;
                case ResourceType.Cloth:
                    return Cloth;
                case ResourceType.Desert:
                    break;
                case ResourceType.Back:
                    break;
                case ResourceType.None:
                    break;
                case ResourceType.Sea:
                    break;
                default:
                    break;
            }
            return 0;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}