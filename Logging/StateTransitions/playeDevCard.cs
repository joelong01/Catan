using System;
using System.Collections.Generic;
using System.Text.Json;

using Catan.Proxy;

namespace Catan10
{
    public class KnightPlayedLog : PlayedDevCardModel
    {
        #region Constructors

        public KnightPlayedLog() : base()
        {
            Action = CatanAction.PlayedDevCard;
            DevCard = DevCardType.Knight;
        }

        #endregion Constructors

        #region Properties

        public int PickedTileIndex { get; set; } = -1;
        public int PreviousTileIndex { get; set; } = -1;
        public ResourceType ResourceAcquired { get; set; }
        public string Victim { get; set; } = "";

        #endregion Properties

        // who got targetted
        // what got stolen
        // where did the Baron land
        // where did the Baron come from
    }

    public class PlayedDevCardModel : LogHeader
    {
        #region Constructors

        public PlayedDevCardModel() : base()
        {
            Action = CatanAction.PlayedDevCard;
        }

        #endregion Constructors

        #region Properties

        public DevCardType DevCard { get; set; } = DevCardType.Unknown;

        #endregion Properties

        #region Methods

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

        #endregion Methods
    }

    public class PlayedMonopoly : PlayedDevCardModel
    {
        #region Constructors

        public PlayedMonopoly() : base()
        {
            DevCard = DevCardType.Monopoly;
        }

        #endregion Constructors

        #region Properties

        public Dictionary<string, int> PlayerToCardCount { get; set; } = new Dictionary<string, int>();
        public ResourceType ResourceType { get; set; } = ResourceType.None;

        #endregion Properties
    }

    public class PlayedPlayedYearOfPlentyLog : PlayedDevCardModel
    {
        #region Constructors

        public PlayedPlayedYearOfPlentyLog() : base()
        {
            DevCard = DevCardType.YearOfPlenty;
        }

        #endregion Constructors

        #region Properties

        public TradeResources Acquired { get; set; } = null;

        #endregion Properties
    }
}
