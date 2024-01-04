using System.ComponentModel;

namespace Catan10
{
    public enum AnimationSpeed { Ultra = 5, SuperFast = 100, VeryFast = 250, Fast = 500, Normal = 1000, Slow = 3000 };

    public enum BodyType
    {
        TradeResources,
        None,
        GameInfo,
        TradeResourcesList
    }

    public enum BuildingLocation { TopRight, MiddleRight, BottomRight, BottomLeft, MiddleLeft, TopLeft, None };

    public enum CatanAction
    {
        Rolled,
        ChangedState,
        ChangedPlayer,
        Dealt,
        CardsLost,
        CardsLostToSeven,
        MissedOpportunity,
        DoneSupplemental,
        DoneResourceAllocation,
        RolledSeven,
        MovingBaron,
        UpdatedRoadState,
        UpdateBuildingState,
        AssignedPirateShip,
        AddPlayer,
        SelectGame,
        InitialAssignBaron,
        None,
        SetFirstPlayer,
        RoadTrackingChanged,
        AddResourceCount,
        ChangedPlayerProperty,
        SetRandomTileToGold,
        ChangePlayerAndSetState,
        Started,
        RandomizeBoard,
        Purchased,
        GameDeleted,
        PlayedDevCard,
        PlayerTrade,
        TradeGold,
        GrantResources,
        CardTaken,
        MeritimeTrade,
        GameCreated,
        Undefined,
        TradeResources,
        PlayedKnight,
        StartGame,
        RollToSeeWhoGoesFirst,
        Undo,
        Redo,
        Verify,
        ShowAllRolls,
        GameJoined,
        DeleteOffer,
        ChangedTradeApproval,
        Ack
    };

    public enum CatanGames { Regular, Expansion, Seafarers, Seafarers4Player };

    //
    // Back is needed because the resource control flips from its back to front..this makes a front look like the back of a dev card
    public enum DevCardType
    {
        [Description("Knight")]
        Knight,

        [Description("Victory Point")]
        VictoryPoint,

        [Description("Year Of Plenty")]
        YearOfPlenty,

        [Description("Road Building")]
        RoadBuilding,

        [Description("Monopoly")]
        Monopoly,

        [Description("")]
        None,

        [Description("")]
        Back
    };



    public enum Entitlement
    {
        [Description("Undefined")]
        Undefined,

        [Description("Dev Card")]
        DevCard,

        [Description("Settlement")]
        Settlement,

        [Description("City")]
        City,

        [Description("Road")]
        Road,

        [Description("Ship")]
        Ship,

        [Description("Buy Or Upgrade")]
        BuyOrUpgradeKnight,

        [Description("Activate")]
        ActivateKnight,

        [Description("Move")]
        MoveKnight,

        [Description("Politics")]
        PoliticsUpgrade,

        [Description("Science")]
        ScienceUpgrade,

        [Description("Trade")]
        TradeUpgrade,

        [Description("Wall")]
        Wall,

        [Description("Destroy City")]
        DestroyCity,
        [Description("Bishop")]
        Bishop,
        [Description("Deserter")]
        Deserter,


        [Description("Inventor")]
        Inventor,
        [Description("Intrigue")]
        Intrigue,
        [Description("Diplomat")]
        Diplomat,

        [Description("Merchant")]
        Merchant,



    }


    public enum GameState
    {
        [Description("Uninitialized")]
        Uninitialized,

        [Description("New Game")]
        WaitingForNewGame,

        [Description("Start Pick Resources")]
        BeginResourceAllocation,

        [Description("Done Adding Players.  Pick Board...")]
        WaitingForPlayers,

        [Description("Accept Board")]
        PickingBoard,

        [Description("Roll...")]
        WaitingForRollForOrder,

        [Description("Order Done")]
        FinishedRollOrder,

        [Description("Finished Building...")]
        AllocateResourceForward,

        [Description("Finished Building...")]
        AllocateResourceReverse,

        [Description("Start Game...")]
        DoneResourceAllocation,

        [Description("Select Roll...")]
        WaitingForRoll,

        [Description("Done - Next Player")]
        WaitingForNext,

        [Description("Done - Next Player")]
        Supplemental,

        [Description("Move Baron")]
        MustMoveBaron,

        [Description("Discard Cards")]
        TooManyCards,

        [Description("Destroy City")]
        MustDestroyCity,

        [Description("Picking Random Gold Cards")]
        PickingRandomGoldTiles,

        [Description("Handling Pirates")]
        HandlePirates,

        [Description("Done Destroying Cities")]
        DoneDestroyingCities
    };

    public enum GameType { Regular, SupplementalBuildPhase, Saved };

    public enum HarborLocation { None = 0x00000000, TopRight = 0x00100000, TopLeft = 0x00010000, BottomRight = 0x00001000, BottomLeft = 0x00000100, Top = 0x00000010, Bottom = 0x00000001 };

    public enum HarborType { Sheep, Wood, Ore, Wheat, Brick, ThreeForOne, None };

    public enum LogState
    {
        Normal,
        Replay,
        Undo,
        ServiceUpdate
    }

    public enum LogType { Normal, Undo, Replay, DoNotLog, DoNotUndo, Redo };

    public enum MoveBaronReason { PlayedDevCard, Rolled7 };

    public enum ResourceType { Sheep, Wood, Ore, Wheat, Brick, GoldMine, Desert, Back, None, Sea, Coin, Cloth, Paper, Politics, Trade, Science, AnyDevCard, VictoryPoint, Invasion };

    public enum RoadLocation { None = -1, Top = 0, TopRight = 1, BottomRight = 2, Bottom = 3, BottomLeft = 4, TopLeft = 5 };

    public enum RoadState { Unowned, Road, Ship };

    public enum TargetWeapon { PirateShip, Baron };

    public enum TileDisplay { Normal, Gold };

    public enum TileLocation { Self = -1, Top = 0, TopRight = 1, BottomRight = 2, Bottom = 3, BottomLeft = 4, TopLeft = 5 };

    public enum TileOrientation { FaceDown, FaceUp, None };
    public enum SpecialDice { Trade=0, Politics=1, Science=2, Pirate=3, None=-1 };

    //
    //  actions are things we can undo

    // typical animation speeds in ms

    public enum UndoOrder { PreviousThenUndo, UndoNoPrevious, None };

    public enum ValidNumbers { Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10, Eleven = 11, Twelve = 12 };
}