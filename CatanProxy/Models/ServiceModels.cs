using System;
using System.Collections.Generic;


namespace CatanSharedModels
{
    /// <summary>
    ///     This enum tells us what the data shape is 
    /// </summary>
    public enum ServiceLogType
    {
        Undefined, Resource, Game, Purchase,
        Trade, TakeCard, MeritimeTrade,
        UpdateTurn,
        Monopoly
    }
    public enum CatanError
    {
        DevCardsSoldOut,
        NoMoreResource,
        LimitExceeded,
        NoGameWithThatName,
        NoPlayerWithThatName,
        NotEnoughResourcesToPurchase,
        MissingData,
        BadTradeResources,
        NoResource,
        BadEntitlement,
        BadParameter,
        BadLogRecord,
        PlayerAlreadyRegistered,
        GameAlreadStarted,
        Unknown,
        InsufficientResource,
        Unexpected,
        NoError,
    }
    /// <summary>
    ///     this enum tells us what the data was used for. We often have data shapes for only one reason...
    /// </summary>
    public enum ServiceAction
    {
        Undefined, Purchased, PlayerAdded, UserRemoved, GameCreated, GameDeleted,
        TradeGold, GrantResources, TradeResources, TakeCard, Refund, MeritimeTrade, UpdatedTurn,
        LostToMonopoly,
        PlayedMonopoly,
        PlayedRoadBuilding,
        PlayedKnight,
        PlayedYearOfPlenty,
        ReturnResources,
        GameStarted
    }
    /// <summary>
    ///     returned by Monitor.  
    ///         Sequence number used to ensure that no records are missed at the client
    ///         Count is used to verify/test marshaling of LogRecords
    ///         LogRecords are the data you actually want
    ///         
    ///         LogRecords is a List<object> so that the Serializer will serialize
    ///         all of the information in the derived classes
    /// </summary>
    public class ServiceLogCollection
    {
        public int SequenceNumber { get; set; }
        public int Count { get; set; }
        public List<object> LogRecords { get; set; }
        public Guid CollectionId { get; set; }
    }



    public class ServiceLogRecord
    {
        public int Sequence { get; set; }
        public Guid LogId { get; set; } = Guid.NewGuid();
        public ServiceLogType LogType { get; set; } = ServiceLogType.Undefined;
        public ServiceAction Action { get; set; } = ServiceAction.Undefined;
        public string PlayerName { get; set; }
        public string RequestUrl { get; set; }
        public CatanRequest UndoRequest { get; set; } = null; // the request to undo this action
        public override string ToString()
        {
            return $"[LogType={LogType}][Player={PlayerName}][Action={Action}][Url={RequestUrl}]";
        }

    }
    public class ResourceLog : ServiceLogRecord
    {
        public PlayerResources PlayerResources { get; set; } // this is not needed for Undo, but is needed for each of the games to update their UI
        public TradeResources TradeResource { get; set; } // needed for Undo

        public ResourceLog() { LogType = ServiceLogType.Resource; }
        public override string ToString()
        {
            return base.ToString() + PlayerResources.ToString();
        }
    }

    public class MonopolyLog : ResourceLog
    {
        public ResourceType ResourceType { get; set; }
        public int Count { get; set; } = 0;
        public MonopolyLog() { }
        public override string ToString()
        {
            return base.ToString() + $"[ResourceType={ResourceType}]";
        }
    }

    public class TurnLog : ServiceLogRecord
    {
        public string NewPlayer { get; set; } = "";
        public TurnLog() { LogType = ServiceLogType.UpdateTurn; Action = ServiceAction.UpdatedTurn; }
    }

    public class TradeLog : ServiceLogRecord
    {
        public TradeLog() { LogType = ServiceLogType.Trade; }
        public TradeResources FromTrade { get; set; }
        public TradeResources ToTrade { get; set; }
        public PlayerResources FromResources { get; set; }
        public PlayerResources ToResources { get; set; }

        public string FromName { get; set; }
        public string ToName { get; set; }

    }
    public class TakeLog : ServiceLogRecord
    {
        public TakeLog() { LogType = ServiceLogType.TakeCard; }
        public ResourceType Taken { get; set; }
        public PlayerResources FromResources { get; set; }
        public PlayerResources ToResources { get; set; }

        public string FromName { get; set; }
        public string ToName { get; set; }

    }


    public class MeritimeTradeLog : ServiceLogRecord
    {
        public MeritimeTradeLog() { LogType = ServiceLogType.MeritimeTrade; Action = ServiceAction.MeritimeTrade; }
        public ResourceType Traded { get; set; }
        public int Cost { get; set; }
        public PlayerResources Resources { get; set; }

    }
    public class PurchaseLog : ServiceLogRecord
    {
        public Entitlement Entitlement { get; set; }
        public PlayerResources PlayerResources { get; set; }
        public PurchaseLog() { LogType = ServiceLogType.Purchase; }
        public override string ToString()
        {
            return $"[Entitlement={Entitlement}]" + base.ToString();
        }
    }
    public class GameLog : ServiceLogRecord
    {
        public ICollection<string> Players { get; set; }
        public GameLog() { LogType = ServiceLogType.Game; }
    }
}
