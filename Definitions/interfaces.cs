using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Catan.Proxy;
using Windows.UI.Xaml.Input;

namespace Catan10
{

   

    public interface ILogController
    {
        Task Do(IGameController gameController, LogHeader logHeader);
        Task Undo(IGameController gameController, LogHeader logHeader);
        Task Redo(IGameController gameController, LogHeader logHeader);
        

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
        CatanGames CatanGame { get; }
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
        bool CanBuild { get; }
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



}
