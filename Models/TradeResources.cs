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

        #endregion Properties

        #region Constructors + Destructors

        public TradeResources()
        {
        }

        public TradeResources(TradeResources tradeResources)
        {
            Wheat = this.Wheat;
            Wood = this.Wood;
            Brick = this.Brick;
            Ore = this.Ore;
            Sheep = this.Sheep;
            GoldMine = this.GoldMine;
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

                case Entitlement.Knight:
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
                GoldMine = a.GoldMine + b.GoldMine
            };
        }


        public bool EqualValue(TradeResources b)
        {
            if (b == null) return false;
            return (
                (this.Wheat == b.Wheat) &&
                (this.Wood == b.Wood) &&
                (this.Ore == b.Ore) &&
                (this.Sheep == b.Sheep) &&
                (this.Brick == b.Brick) &&
                (this.GoldMine == b.GoldMine));
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

                case ResourceType.Desert:
                case ResourceType.Back:
                case ResourceType.None:
                case ResourceType.Sea:
                default:
                    return 0;
            }
        }

        public bool Equivalent(TradeResources tradeResources)
        {
            if (Wheat != tradeResources.Wheat || Wood != tradeResources.Wood || Ore != tradeResources.Ore ||
                Sheep != tradeResources.Sheep || Brick != tradeResources.Brick || GoldMine != tradeResources.GoldMine)
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
                GoldMine = -GoldMine
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
            return $"[Count={Count}][Ore={Ore}][Brick={Brick}][Wheat={Wheat}][Wood={Wood}][Sheep={Sheep}][Gold={GoldMine}]";
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