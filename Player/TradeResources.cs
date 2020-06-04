using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Catan10
{
    public class TradeResources : INotifyPropertyChanged
    {
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        private int _brick = 0;

        private int _goldMine = 0;

        private int _ore = 0;

        private int _sheep = 0;

        private int _wheat = 0;

        private int _wood = 0;

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

        public void Add(ResourceType resourceType, int toAdd)
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
                    Contract.Assert(false, "Bad pass to Add!");
                    break;
            }
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

        public override string ToString()
        {
            return $"[Count={Count}][Ore={Ore}][Brick={Brick}][Wheat={Wheat}][Wood={Wood}][Sheep={Sheep}]";
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

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
    }
}
