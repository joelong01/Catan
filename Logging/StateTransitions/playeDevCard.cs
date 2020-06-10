using System;
using System.Collections.Generic;
using System.Text.Json;

using Catan.Proxy;

namespace Catan10
{
    public class KnightPlayedLog : PlayedDevCardModel
    {
        public int PickedTileIndex { get; set; } = -1;

        public int PreviousTileIndex { get; set; } = -1;

        public ResourceType ResourceAcquired { get; set; }

        public string Victim { get; set; } = "";

        public KnightPlayedLog() : base()
        {
            Action = CatanAction.PlayedDevCard;
            DevCard = DevCardType.Knight;
        }

        // who got targetted
        // what got stolen
        // where did the Baron land
        // where did the Baron come from
    }

    public class PlayedDevCardModel : LogHeader
    {
        public DevCardType DevCard { get; set; } = DevCardType.None;

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
                case DevCardType.None:
                case DevCardType.VictoryPoint:
                default:
                    break;
            }

            return null;
        }
    }

    public class PlayedMonopoly : PlayedDevCardModel
    {
        public Dictionary<string, int> PlayerToCardCount { get; set; } = new Dictionary<string, int>();

        public ResourceType ResourceType { get; set; } = ResourceType.None;

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
}