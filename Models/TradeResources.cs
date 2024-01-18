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
        private int _politics = 0;
        private int _trade = 0;
        private int _science = 0;
        private int _victoryPoint = 0;
        private int _anyDevcard = 0;
        public int Trade
        {
            get
            {
                return _trade;
            }
            set
            {
                if (value != _trade)
                {
                    _trade = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }

        public int Politics
        {
            get
            {
                return _politics;
            }
            set
            {
                if (value != _politics)
                {
                    _politics = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }

        public int Science
        {
            get
            {
                return _science;
            }
            set
            {
                if (value != _science)
                {
                    _science = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
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
                    NotifyPropertyChanged("Count");
                }
            }
        }

        public int AnyDevCard
        {
            get
            {
                return _anyDevcard;
            }
            set
            {
                if (value != _anyDevcard)
                {
                    _anyDevcard = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Count");
                }
            }
        }
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
        public int Count => Wheat + Wood + Brick + Ore + Sheep + GoldMine + Cloth + Coin + Paper + VictoryPoint + Politics + Science + Trade + AnyDevCard;

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
                if (Science != 0) list.Add(ResourceType.Science);
                if (Trade != 0) list.Add(ResourceType.Trade);
                if (Politics != 0) list.Add(ResourceType.Politics);
                if (VictoryPoint != 0) list.Add(ResourceType.VictoryPoint);
                if (AnyDevCard != 0) list.Add(ResourceType.AnyDevCard);
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

            Politics = tradeResources.Politics;
            Trade = tradeResources.Trade;
            Science = tradeResources.Science;
            VictoryPoint = tradeResources.VictoryPoint;
            AnyDevCard = tradeResources.AnyDevCard;
        }

        public static TradeResources GetEntitlementCost(PlayerModel player, Entitlement entitlement)
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
                case Entitlement.UpgradeKnight:
                case Entitlement.BuyKnight:
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
                case Entitlement.PoliticsUpgrade:
                    cost = new TradeResources()
                    {
                        Coin = player.GameData.PoliticsRank
                    };
                    break;

                case Entitlement.TradeUpgrade:
                    cost = new TradeResources()
                    {
                        Cloth = player.GameData.TradeRank
                    };
                    break;

                case Entitlement.ScienceUpgrade:
                    cost = new TradeResources()
                    {
                        Paper = player.GameData.ScienceRank
                    };
                    break;
                case Entitlement.Wall:
                    cost = new TradeResources()
                    {
                        Brick = 2
                    };
                    break;
                case Entitlement.Undefined:
                default:
                    cost = new TradeResources() { };
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
                case ResourceType.Politics:
                case ResourceType.Science:
                case ResourceType.Trade:
                case ResourceType.VictoryPoint:
                case ResourceType.AnyDevCard:
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
                Cloth = a.Cloth + b.Cloth,

                Politics = a.Politics + b.Politics,
                Trade = a.Trade + b.Trade,
                Science = a.Science + b.Science,
                VictoryPoint = a.VictoryPoint + b.VictoryPoint,
                AnyDevCard = a.AnyDevCard + b.AnyDevCard

            };
        }

        public static TradeResources TradeResourcesForRedDie(SpecialDice roll)
        {
            TradeResources tr = new TradeResources();
            switch (roll)
            {
                case SpecialDice.Trade:
                    tr.Trade++;
                    break;
                case SpecialDice.Politics:
                    tr.Science++;
                    break;
                case SpecialDice.Science:
                    tr.Politics++;
                    break;
                case SpecialDice.Pirate:
                    break;
                case SpecialDice.None:
                    break;
            }
            return tr;
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
                case ResourceType.Politics:
                    Politics += toAdd;
                    break;
                case ResourceType.Science:
                    Science += toAdd;
                    break;
                case ResourceType.Trade:
                    Trade += toAdd;
                    break;
                case ResourceType.VictoryPoint:
                    VictoryPoint += toAdd;
                    break;
                case ResourceType.AnyDevCard:
                    AnyDevCard += toAdd;
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

        public bool CanAfford(PlayerModel player, Entitlement entitlement)
        {
            TradeResources cost = TradeResources.GetEntitlementCost(player, entitlement);
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
                case ResourceType.Politics:
                    return Politics;
                case ResourceType.Science:
                    return Science;
                case ResourceType.Trade:
                    return Trade;
                case ResourceType.VictoryPoint:
                    return VictoryPoint;
                case ResourceType.AnyDevCard:
                    return AnyDevCard;
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
                ( this.Paper == b.Paper ) &&
                ( this.Politics == b.Politics ) &&
                ( this.Science == b.Science ) &&
                ( this.Trade == b.Trade ) &&
                ( this.VictoryPoint == b.VictoryPoint ) &&
                ( this.AnyDevCard == b.AnyDevCard ) );
        }

        public bool Equivalent(TradeResources tradeResources)
        {
            if (Wheat != tradeResources.Wheat || Wood != tradeResources.Wood || Ore != tradeResources.Ore ||
                Sheep != tradeResources.Sheep || Brick != tradeResources.Brick || GoldMine != tradeResources.GoldMine ||
                Coin != tradeResources.Coin || Paper != tradeResources.Paper || Cloth != tradeResources.Cloth ||
                Politics != tradeResources.Politics || Science != tradeResources.Science || Trade != tradeResources.Trade ||
                VictoryPoint != tradeResources.VictoryPoint || AnyDevCard != tradeResources.AnyDevCard)
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
                Trade = -Trade,
                Politics = -Politics,
                Science = -Science,
                VictoryPoint = -VictoryPoint,
                AnyDevCard = -AnyDevCard,
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
                case ResourceType.Politics:
                    Politics = value;
                    break;
                case ResourceType.Science:
                    Science = value;
                    break;
                case ResourceType.Trade:
                    Trade = value;
                    break;
                case ResourceType.VictoryPoint:
                    VictoryPoint = value;
                    break;
                case ResourceType.AnyDevCard:
                    AnyDevCard = value;
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
            for (int i = 0; i < Science; i++)
            {
                list.Add(ResourceType.Science);
            }
            for (int i = 0; i < Trade; i++)
            {
                list.Add(ResourceType.Trade);
            }
            for (int i = 0; i < Politics; i++)
            {
                list.Add(ResourceType.Politics);
            }
            for (int i = 0; i < VictoryPoint; i++)
            {
                list.Add(ResourceType.VictoryPoint);
            }
            for (int i = 0; i < AnyDevCard; i++)
            {
                list.Add(ResourceType.AnyDevCard);
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
                case ResourceType.Politics:
                    return Politics;
                case ResourceType.Trade:
                    return Trade;
                case ResourceType.Science:
                    return Science;
                case ResourceType.VictoryPoint:
                    return VictoryPoint;
                case ResourceType.AnyDevCard:
                    return AnyDevCard;
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

    }
}