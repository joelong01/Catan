
namespace Catan10
{
    public enum TileDisplay { Normal, Gold };

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
        AssignedBaron,
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
        Redo
    };
    public enum CatanGames { Regular, Expansion, Seafarers, Seafarers4Player };
    public enum TileOrientation { FaceDown, FaceUp, None };
    public enum HarborType { Sheep, Wood, Ore, Wheat, Brick, ThreeForOne, Uninitialized, None };
    public enum Entitlement { Undefined, DevCard, Settlement, City, Road }
    
    public enum GameState
    {
        Uninitialized,                   
        WaitingForNewGame,               
        WaitingForStart,
        WaitingForPlayers,
        WaitingForRollForOrder,
        AllocateResourceForward,         
        AllocateResourceReverse,         
        DoneResourceAllocation,          
        WaitingForRoll,
        WaitingForNext,
        Supplemental,

        Targeted,                        
        LostToCardsLikeMonopoly,                 
        DoneSupplemental,                
        LostCardsToSeven,                
        MissedOpportunity,               
        GamePicked,                      
        MustMoveBaron,                   
        Unknown,
        PickingBoard,
    };
    public enum ResourceType { Sheep, Wood, Ore, Wheat, Brick, GoldMine, Desert, Back, None, Sea };
    //
    // Back is needed because the resource control flips from its back to front..this makes a front look like the back of a dev card
    public enum DevCardType { Knight, VictoryPoint, YearOfPlenty, RoadBuilding, Monopoly, Unknown, Back };
    public enum BodyType
    {
        TradeResources,
        None,
        GameInfo,
        TradeResourcesList
    }

    //
    //  actions are things we can undo

    public enum AnimationSpeed { Ultra = 5, SuperFast = 100, VeryFast = 250, Fast = 500, Normal = 1000, Slow = 3000 }; // typical animation speeds in ms
    public enum UndoOrder { PreviousThenUndo, UndoNoPrevious, None };
    public enum RoadState { Unowned, Road, Ship };
    

    public enum HarborLocation { None = 0x00000000, TopRight = 0x00100000, TopLeft = 0x00010000, BottomRight = 0x00001000, BottomLeft = 0x00000100, Top = 0x00000010, Bottom = 0x00000001 };
    public enum RoadLocation { None = -1, Top = 0, TopRight = 1, BottomRight = 2, Bottom = 3, BottomLeft = 4, TopLeft = 5 };
    public enum TileLocation { Self = -1, Top = 0, TopRight = 1, BottomRight = 2, Bottom = 3, BottomLeft = 4, TopLeft = 5 };
    public enum ValidNumbers { Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10, Eleven = 11, Twelve = 12 };
    public enum BuildingLocation { TopRight, MiddleRight, BottomRight, BottomLeft, MiddleLeft, TopLeft, None };


    public enum GameType { Regular, SupplementalBuildPhase, Saved };
    public enum TargetWeapon { PirateShip, Baron };

}
