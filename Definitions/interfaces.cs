﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Catan.Proxy;
using Windows.UI.Xaml.Input;

namespace Catan10
{
    //
    //  this interface is implemented by the Controls that wrap the SimpleHexPanel
    //  and exposes the data needed by the Views/Pages
    interface ICatanGameData
    {
        CatanGames CatanGame { get; }
        string Description { get; }
        List<TileCtrl> DesertTiles { get; }
        GameType GameType { get; }
        CatanHexPanel HexPanel { get; }
        int Index { get; }
        int MaxCities { get; }
        int MaxRoads { get; }
        int MaxSettlements { get; }
        int MaxShips { get; }
        List<TileCtrl> Tiles { get; }
    }

    public interface ICatanSettings
    {
        bool AnimateFade { get; set; }
        int AnimationSpeedBase { get; set; }
        int FadeSeconds { get; set; }
        bool ResourceTracking { get; set; }
        bool RotateTile { get; set; }
        bool ShowStopwatch { get; set; }
        bool UseClassicTiles { get; set; }
        bool UseRandomNumbers { get; set; }
        bool ValidateBuilding { get; set; }
        double Zoom { get; set; }
        void Close();

        Task Explorer();

        Task NewGame();

        Task OpenSavedGame();

        Task ResetGridLayout();

        Task<bool> Reshuffle();

        Task RotateTiles();

        Task SettingChanged();
        Task Winner();
    }

    public interface IGameCallback
    {

        bool CanBuildRoad { get; }

        Task AddLogEntry(PlayerModel player, GameState state, CatanAction action, bool UIVisible, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0);

        Task BuildingStateChanged(PlayerModel player, BuildingCtrl settlement, BuildingState oldState);

        bool BuildingStateChangeOk(BuildingCtrl building);

        BuildingState ValidateBuildingLocation(BuildingCtrl sender);

        void RoadEntered(RoadCtrl road, PointerRoutedEventArgs e);

        void RoadExited(RoadCtrl road, PointerRoutedEventArgs e);

        void RoadPressed(RoadCtrl road, PointerRoutedEventArgs e);

        void TileRightTapped(TileCtrl tile, RightTappedRoutedEventArgs rte);
    }

    public interface IGameController
    {
        CatanGames CatanGame { get; set; }

        /// <summary>
        ///     The current state of the game
        /// </summary>
        GameState CurrentGameState { get; }

        /// <summary>
        ///     The current player in the game
        /// </summary>
        PlayerModel CurrentPlayer { get; }

        List<int> CurrentRandomGoldTiles { get; }

        List<int> HighlightedTiles { get; }

        bool IsServiceGame { get; }

        NewLog Log { get; }

        List<int> NextRandomGoldTiles { get; }

        PlayerModel TheHuman { get; }

        /// <summary>
        ///     Adds a player to the game.  if the Player is already in the game, return false.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        Task AddPlayer(AddPlayerLog playerLogHeader);

        Task ChangePlayer(ChangePlayerLog log);

        void CompleteRedo();

        void CompleteUndo();

        RandomBoardSettings CurrentRandomBoard();

        RandomBoardSettings GetRandomBoard();
        Task SetRoadState(UpdateRoadLog updateRoadModel);

        /// <summary>
        ///     Given a playerName, return the Model by looking up in the AllPlayers collection
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        PlayerModel NameToPlayer(string playerName);
        Task UndoSetRoadState(UpdateRoadLog updateRoadModel);
        Task<bool> PostMessage(LogHeader logHeader, CatanMessageType normal);

        Task<bool> RedoAsync();

        Task SetRandomBoard(RandomBoardLog randomBoard);

        Task SetState(SetStateLog log);

        Task StartGame(StartGameLog model);

        Task SynchronizedRoll(SynchronizedRollLog log);

        Task UndoAddPlayer(AddPlayerLog playerLogHeader);

        Task<bool> UndoAsync();

        Task UndoChangePlayer(ChangePlayerLog log);

        Task UndoSetRandomBoard(RandomBoardLog logHeader);
        Task UndoSetState(SetStateLog setStateLog);
        Task UpdateBuilding(UpdateBuildingLog updateBuildingLog);
        Task UndoUpdateBuilding(UpdateBuildingLog updateBuildingLog);
        List<PlayerModel> PlayingPlayers { get; }
    }


    public interface IGameViewCallback
    {
        void OnGridLeftTapped(TileCtrl tile, TappedRoutedEventArgs e);
        void OnGridRightTapped(TileCtrl tile, RightTappedRoutedEventArgs e);
        void OnHarborRightTapped(TileCtrl tileCtrl, HarborLocation location, RightTappedRoutedEventArgs e);

        void OnTileDoubleTapped(object sender, DoubleTappedRoutedEventArgs e);
    }

    public interface ILogController
    {
        Task Do(IGameController gameController);
        Task Redo(IGameController gameController);

        Task Undo(IGameController gameController);
    }
    public interface ITileControlCallback
    {

        void TileRightTapped(TileCtrl tile, RightTappedRoutedEventArgs rte);

    }
}
