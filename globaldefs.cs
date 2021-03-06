﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Input;

namespace Catan10
{
    public class RandomLists
    {
        public List<int> TileList { get; set; } = null;
        public List<int> NumberList { get; set; } = null;

        public RandomLists(){}
        public RandomLists(string saved)
        {
            Deserialize(saved);
        }
    
        public RandomLists(TileGroup tg)
        {
            TileList = tg.RandomTileList;
            NumberList = tg.RandomNumbersList;
        }


        public string Serialize()
        {
            return JsonSerializer.Serialize<RandomLists>(this);
        }

        public static RandomLists Deserialize(string saved)
        {
            return JsonSerializer.Deserialize<RandomLists>(saved);

        }


    }
    public class RandomBoardSettings
    {
         //
         // every TileGroup has a list that says where to put the tiles
         // and another list that says what number to put on the tiles
         //
         // the int here is the TileGroup Index - System.Text.Json currently only Deserializes Dictionaries keyed by strings.
         //
        public Dictionary<string, RandomLists> TileGroupToRandomListsDictionary { get; set; } = new Dictionary<string, RandomLists>();

        //
        //  every Board has a random list of harbors
        public List<int> RandomHarborTypeList { get; set; } = null;
        public RandomBoardSettings() { }

        public override string ToString()
        {
            return Serialize();
        }

        public RandomBoardSettings(Dictionary<string, RandomLists> Tiles, List<int> Harbors)
        {
            RandomHarborTypeList = Harbors;
            TileGroupToRandomListsDictionary = Tiles;
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize<RandomBoardSettings>(this);           
        }

        public static RandomBoardSettings Deserialize(string saved)
        {
            return JsonSerializer.Deserialize<RandomBoardSettings>(saved);

        }
        
        

    }


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
        Rolled,                                   // 0
        ChangedState,                             // 1 
        ChangedPlayer,                            // 2        
        Dealt,                                    // 3
        CardsLost,                      // 4
        CardsLostToSeven,                         // 5
        MissedOpportunity,                        // 6
        DoneSupplemental,                         // 7
        DoneResourceAllocation,                   // 8
        PlayedKnight,                             // 9
        RolledSeven,                              // 10
        AssignedBaron,                            // 11
        UpdatedRoadState,                         // 12
        UpdateBuildingState,                    // 13
        AssignedPirateShip,                       // 14
        AddPlayer,                                // 15
        RandomizeTiles,                         // 16
        AssignHarbors,                            // 17
        SelectGame,                               // 18
        AssignRandomTiles,                        // 19
        InitialAssignBaron,                       // 20
        None,                                      // 21
        SetFirstPlayer,                            // 22
        RoadTrackingChanged,                         // 23
        AddResourceCount,
        ChangedPlayerProperty,
        SetRandomTileToGold,
        ChangePlayerAndSetState,
        Started
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






    public enum DevCardType { Knight, VictoryPoint, YearOfPlenty, RoadBuilding, Monopoly, Unknown, Back };
    public enum ResourceType { Sheep, Wood, Ore, Wheat, Brick, Desert, Back, None, Sea, GoldMine };
    public enum TileOrientation { FaceDown, FaceUp, None };
    public enum HarborType { Sheep, Wood, Ore, Wheat, Brick, ThreeForOne, Uninitialized, None };
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

        
        readonly string[] _settings = new string[] { "FadeSeconds", "AnimateFade", "RotateTile", "Zoom", "ShowStopwatch", "UseClassicTiles", "AnimationSpeed", "ResourceTracking", "UseRandomNumbers", "ValidateBuilding", };

        public Settings()
        {
            //RotateTile = false;
            //AnimateFade = true;
            //FadeSeconds = 5;
            //Zoom = 1.0;
            //UseClassicTiles = true;
            //AnimationSpeed = 3;
            //ShowStopwatch = true;
            //ResourceTracking = true;
        }
        public string Serialize()
        {
            string settings = StaticHelpers.SerializeObject<Settings>(this, _settings, "=", StaticHelpers.lineSeperator);
            string gridPositions = "";
            foreach (GridPosition gp in GridPositions)
            {
                gridPositions += gp.Serialize() + "\r\n";
            }
            string toSave = string.Format($"[Settings]\r\n{settings}\n[GridPositions]\r\n{gridPositions}");
            return toSave;
        }

        public bool Deserialize(string s)
        {
            Dictionary<string, string> sections = StaticHelpers.GetSections(s);
            StaticHelpers.DeserializeObject<Settings>(this, sections["Settings"], "=", StaticHelpers.lineSeperator);
            string[] positions = sections["GridPositions"].Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            GridPositions.Clear();
            foreach (string line in positions)
            {
                GridPosition gp = new GridPosition(line);
                GridPositions.Add(gp);

            }
            return true;
        }

        public async Task LoadSettings(string filename)
        {
            try
            {
                StorageFolder folder = await StaticHelpers.GetSaveFolder();
                StorageFile file = await folder.GetFileAsync(filename);
                string contents = await FileIO.ReadTextAsync(file);
                Deserialize(contents);
            }
            catch
            {

            }
        }

        public async Task SaveSettings(string filename)
        {
            try
            {

                string saveString = Serialize();
                if (saveString == "")
                {
                    return;
                }

                StorageFolder folder = await StaticHelpers.GetSaveFolder();
                CreationCollisionOption option = CreationCollisionOption.ReplaceExisting;
                StorageFile file = await folder.CreateFileAsync(filename, option);
                await FileIO.WriteTextAsync(file, saveString);


            }
            catch (Exception exception)
            {

                string s = StaticHelpers.GetErrorMessage($"Error saving to file {filename}", exception);
                await StaticHelpers.ShowErrorText(s);

            }
        }





    }



}
