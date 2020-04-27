using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI.Xaml.Input;

namespace Catan10
{
  
    public enum GameState
    {
        Uninitialized,                      // 0
        WaitingForNewGame,                  // 1 
        Starting,                           // 2
        Dealing,                            // 3
        WaitingForStart,                    // 4
        AllocateResourceForward,            // 5
        AllocateResourceReverse,            // 6
        DoneResourceAllocation,             // 7
        WaitingForRoll,                     // 8
        Targeted,                           // 9
        LostToCardsLikeMonopoly,            // 10
        Supplemental,                       // 11
        DoneSupplemental,                   // 12
        WaitingForNext,                     // 13
        LostCardsToSeven,                   // 14
        MissedOpportunity,                  // 15
        GamePicked,                         // 16
        MustMoveBaron,                       // 17
        Unknown,
    };

    //
    //  actions are things we can undo
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
        PlayedKnight,              
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
        RandomizeBoard
    };
    public enum AnimationSpeed { Ultra = 50, SuperFast = 100, VeryFast = 250, Fast = 500, Normal = 1000, Slow = 3000 }; // typical animation speeds in ms
    public enum UndoOrder { PreviousThenUndo, UndoNoPrevious, None };
    public enum RoadState { Unowned, Road, Ship };
    public enum PlayerPosition { BottomLeft, TopLeft, TopRight, BottomRight, Left, Right, None }; // the order is semantic!
    public class UndoEventArgs
    {
        public UndoOrder UndoOrder { get; set; } = UndoOrder.None;
    }

    public enum LayoutDirection { ClockWise, CounterClockwise };



    public class ChangeGameEventArgs
    {
        public StorageFile File { get; set; }
        public ChangeGameEventArgs(StorageFile f)
        {
            File = f;
        }
    }

    public interface IGameViewCallback
    {
        void OnGridLeftTapped(TileCtrl tile, TappedRoutedEventArgs e);
        void OnGridRightTapped(TileCtrl tile, RightTappedRoutedEventArgs e);
        void OnTileDoubleTapped(object sender, DoubleTappedRoutedEventArgs e);
        void OnHarborRightTapped(TileCtrl tileCtrl, HarborLocation location, RightTappedRoutedEventArgs e);
    }





    //
    //  this interface is implemented by the Controls that wrap the SimpleHexPanel
    //  and exposes the data needed by the Views/Pages
    interface ICatanGameData
    {
        string Description { get; }
        GameType GameType { get; }
        int MaxCities { get; }
        int MaxRoads { get; }
        int MaxSettlements { get; }
        int MaxShips { get; }
        List<TileCtrl> Tiles { get; }
        List<TileCtrl> DesertTiles { get; }
        CatanHexPanel HexPanel { get; }
        int Index { get; }
    }

    public interface ITileControlCallback
    {

        void TileRightTapped(TileCtrl tile, RightTappedRoutedEventArgs rte);

    }

    public interface IGameCallback
    {

        void TileRightTapped(TileCtrl tile, RightTappedRoutedEventArgs rte);
        bool CanBuild();
        void RoadEntered(RoadCtrl road, PointerRoutedEventArgs e);
        void RoadExited(RoadCtrl road, PointerRoutedEventArgs e);
        void RoadPressed(RoadCtrl road, PointerRoutedEventArgs e);



        Task BuildingStateChanged(BuildingCtrl settlement, BuildingState oldState, LogType logType);

        Task AddLogEntry(PlayerModel player, GameState state, CatanAction action, bool UIVisible, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0);
        Tuple<bool, bool> IsValidBuildingLocation(BuildingCtrl sender);
        bool BuildingStateChangeOk(BuildingCtrl building);
    }

    public interface ICatanSettings
    {
        bool RotateTile { get; set; }
        bool AnimateFade { get; set; }
        int FadeSeconds { get; set; }
        double Zoom { get; set; }
        bool ShowStopwatch { get; set; }
        bool UseClassicTiles { get; set; }
        int AnimationSpeedBase { get; set; }
        bool ResourceTracking { get; set; }
        bool UseRandomNumbers { get; set; }
        bool ValidateBuilding { get; set; }

        Task SettingChanged();

        Task NewGame();
        Task OpenSavedGame();
        Task<bool> Reshuffle();
        Task Explorer();
        Task RotateTiles();
        Task Winner();

        void Close();

        Task ResetGridLayout();
    }








    public enum HarborLocation { None = 0x00000000, TopRight = 0x00100000, TopLeft = 0x00010000, BottomRight = 0x00001000, BottomLeft = 0x00000100, Top = 0x00000010, Bottom = 0x00000001 };
    public enum RoadLocation { None = -1, Top = 0, TopRight = 1, BottomRight = 2, Bottom = 3, BottomLeft = 4, TopLeft = 5 };
    public enum TileLocation { Self = -1, Top = 0, TopRight = 1, BottomRight = 2, Bottom = 3, BottomLeft = 4, TopLeft = 5 };
    public enum ValidNumbers { Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10, Eleven = 11, Twelve = 12 };
    public enum BuildingLocation { TopRight, MiddleRight, BottomRight, BottomLeft, MiddleLeft, TopLeft, None };

    public enum GameType { Regular, SupplementalBuildPhase, Saved };
    public enum TargetWeapon { PirateShip, Baron };

    public class Settings
    {
        public bool RotateTile { get; set; } = false;
        public bool AnimateFade { get; set; } = true;
        public int FadeSeconds { get; set; } = 3;
        public double Zoom { get; set; } = 1.0;
        public bool ShowStopwatch { get; set; } = true;
        public bool UseClassicTiles { get; set; } = true;
        public int AnimationSpeed { get; set; } = 3;
        public bool ResourceTracking { get; set; } = true;
        public bool UseRandomNumbers { get; set; } = true;
        public bool ValidateBuilding { get; set; } = true;
        public List<GridPosition> GridPositions { get; set; } = new List<GridPosition>();




        public Settings()
        {

        }
        public string Serialize()
        {
            return CatanProxy.Serialize(this);
        }

        public static Settings Deserialize(string s)
        {

            return CatanProxy.Deserialize<Settings>(s);
        }

    }



}
