﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.UI.Xaml.Input;

namespace Catan10
{
    //
    //  this interface is implemented by the Controls that wrap the SimpleHexPanel
    //  and exposes the data needed by the Views/Pages
    internal interface ICatanGameData
    {
        CatanGames CatanGame { get; }
        string Description { get; }
        List<TileCtrl> DesertTiles { get; }
        CatanGameData GameData { get; }
        GameType GameType { get; }
        CatanHexPanel HexPanel { get; }
        int Index { get; }

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
        Task BuildingStateChanged(PlayerModel player, BuildingCtrl settlement, BuildingState oldState);

        bool BuildingStateChangeOk(BuildingCtrl building);

        bool CanBuildRoad();

        void RoadEntered(RoadCtrl road, PointerRoutedEventArgs e);

        void RoadExited(RoadCtrl road, PointerRoutedEventArgs e);

        void RoadPressed(RoadCtrl road, PointerRoutedEventArgs e);

        void TileRightTapped(TileCtrl tile, RightTappedRoutedEventArgs rte);

        BuildingState ValidateBuildingLocation(BuildingCtrl sender);
    }

    public interface IGameController
    {
        bool AutoRespondAndTheHuman { get; }
        CatanGames CatanGame { get; set; }

        /// <summary>
        ///     The current state of the game
        /// </summary>
        GameState CurrentGameState { get; }

        /// <summary>
        ///     The current player in the game
        /// </summary>
        PlayerModel CurrentPlayer { get; set; }

        List<int> CurrentRandomGoldTiles { get; }
        GameContainerCtrl GameContainer { get; }
        CatanGameData GameData { get; }
        List<int> HighlightedTiles { get; }

        bool IsServiceGame { get; }

        Log Log { get; }

        MainPageModel MainPageModel { get; }

        List<PlayerModel> PlayingPlayers { get; }

        IRollLog RollLog { get; }
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

        Task<bool> DetermineRollOrder(RollOrderLog log);

        RollState GetNextRollState();

        RandomBoardSettings GetRandomBoard();

        Task HideRollsInPublicUi();

        /// <summary>
        ///     Given a playerName, return the Model by looking up in the AllPlayers collection
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        PlayerModel NameToPlayer(string playerName);

        Task<bool> PostMessage(LogHeader logHeader, CatanMessageType normal);

        DevCardType PurchaseNextDevCard();

        Task PushRollState(RollState rollState);

        Task<bool> RedoAsync();

        void ResetAllBuildings();

        Task ResetRandomGoldTiles();

        Task ResetRollControl();

        (TradeResources Granted, TradeResources Baroned) ResourcesForRoll(PlayerModel player, int roll);

        void SetHighlightedTiles(int roll);

        Task SetRandomBoard(RandomBoardLog randomBoard);

        Task SetRandomTileToGold(List<int> goldTiles);

        Task SetRoadState(UpdateRoadLog updateRoadModel);

        Task SetState(SetStateLog log);

        void ShowRollsInPublicUi();

        Task StartGame(NewGameLog model);

        void StopHighlightingTiles();

        Task TellServiceGameStarted();

        TileCtrl TileFromIndex(int targetTile);

        Task UndoAddPlayer(AddPlayerLog playerLogHeader);

        Task<bool> UndoAsync();

        Task UndoChangePlayer(ChangePlayerLog log);

        Task UndoSetRandomBoard(RandomBoardLog logHeader);

        Task UndoSetRoadState(UpdateRoadLog updateRoadModel);

        Task UndoSetState(SetStateLog setStateLog);

        Task UndoUpdateBuilding(UpdateBuildingLog updateBuildingLog);

        Task UpdateBuilding(UpdateBuildingLog updateBuildingLog);
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

    public interface IMessageDeserializer
    {
        LogHeader Deserialize(string json);
    }

    public interface ITileControlCallback
    {
        void TileRightTapped(TileCtrl tile, RightTappedRoutedEventArgs rte);
    }
}
