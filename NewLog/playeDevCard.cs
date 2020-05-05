using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Catan10
{
    public class PlayedDevCardModel : LogHeader
    {
        public DevCardType DevCard { get; set; } = DevCardType.Unknown;
        public PlayedDevCardModel() : base()
        {
            Action = CatanAction.PlayedDevCard;
        }
        //
        //  this allows us to deal with the polymorphic deserialization of these log records
        //  "json" has the full object graph of the LogHeader, PlayedDevCardModel, and any 
        //  derived class
        //
        static public PlayedDevCardModel Deserialize(object unparsedJson)
        {
            string action = ((JsonElement)unparsedJson).GetProperty("devCard").GetString();
            if (String.IsNullOrEmpty(action)) return null;
            bool ret = Enum.TryParse(action, true, out DevCardType devCard);
            if (!ret) return null;
            string json = unparsedJson.ToString();
            switch (devCard)
            {
                case DevCardType.Knight:
                    KnightPlayedLog pk = CatanProxy.Deserialize<KnightPlayedLog>(json);
                    return pk as PlayedDevCardModel;
                case DevCardType.YearOfPlenty:
                    PlayedPlayedYearOfPlentyLog yop = CatanProxy.Deserialize<PlayedPlayedYearOfPlentyLog>(json);
                    return yop as PlayedDevCardModel;
                case DevCardType.RoadBuilding:
                    return CatanProxy.Deserialize<PlayedDevCardModel>(json);
                case DevCardType.Monopoly:
                    PlayedMonopoly mono = CatanProxy.Deserialize<PlayedMonopoly>(json);
                    return mono as PlayedMonopoly;
                case DevCardType.Back:
                case DevCardType.Unknown:
                case DevCardType.VictoryPoint:
                default:
                    break;
            }

            return null;
        }
    }

    public class PlayedMonopoly : PlayedDevCardModel
    {
        public ResourceType ResourceType { get; set; } = ResourceType.None;
        public Dictionary<string, int> PlayerToCardCount { get; set; } = new Dictionary<string, int>();
        public PlayedMonopoly() : base()
        {
            DevCard = DevCardType.Monopoly;
        }
    }

    public class PlayedPlayedYearOfPlentyLog : PlayedDevCardModel
    {
        public TradeResources Acquired { get; set; } = null;
        public PlayedPlayedYearOfPlentyLog() : base()
        {
            DevCard = DevCardType.YearOfPlenty;
        }

    }

    public class KnightPlayedLog : PlayedDevCardModel
    {
        public string Victim { get; set; } = "";                // who got targetted
        public ResourceType ResourceAcquired { get; set; }      // what got stolen
        public int PreviousTileIndex { get; set; } = -1;        // where did the Baron land
        public int PickedTileIndex { get; set; } = -1;          // where did the Baron come from
        public KnightPlayedLog() : base()
        {
            Action = CatanAction.PlayedDevCard;
            DevCard = DevCardType.Knight;
        }
    }
}
