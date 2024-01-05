using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Catan.Proxy;
using Catan10.CatanService;
using Windows.Foundation;
using Windows.UI.Xaml.Input;

namespace Catan10
{
    public delegate void BroadcastMessageReceivedHandler(CatanMessage message);


    public delegate void DeleteGameHandler(GameInfo gameInfo, string by);


    public delegate void GameLifeTimeHandler(GameInfo gameInfo, string gameName);

    public delegate void PrivateMessageReceivedHandler(CatanMessage message);

    public interface ICatanService
    {
       

        /// <summary>
        ///     a message is recieved that was sent to all clients
        /// </summary>
        event BroadcastMessageReceivedHandler OnBroadcastMessageReceived;

        /// <summary>
        ///    a game as created
        /// </summary>
        event GameLifeTimeHandler OnGameCreated;

        /// <summary>
        ///    a game was deleted
        /// </summary>
        event DeleteGameHandler OnGameDeleted;

        /// <summary>
        ///     a game was joined.  will be sent to the one that joined the game
        /// </summary>
        event GameLifeTimeHandler OnGameJoined;

        /// <summary>
        ///     a game was deleted.  will be sent to the one that joined the game
        /// </summary>
        event GameLifeTimeHandler OnGameLeft;

        /// <summary>
        ///     a message was sent to only this client
        /// </summary>
        event PrivateMessageReceivedHandler OnPrivateMessage;


        Task CreateGame(GameInfo gameInfo);
        Task DisposeAsync();

        /// <summary>
        ///     Tell the service to delete the game with this ID
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns></returns>
        Task DeleteGame(GameInfo gameInfo, string by);

        Task<List<GameInfo>> GetAllGames();

        Task<List<string>> GetAllPlayerNames(Guid gameId);

        /// <summary>
        ///     Connect to the service and start listening for updates.  events will fire.
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="gameInfo"></param>
        /// <returns></returns>
        Task Initialize(string hostName, ICollection<CatanMessage> messageLog, string playerName);

        Task<GameInfo> JoinGame(GameInfo info, string playerName);

        /// <summary>
        ///     leaves the game and returns the list of players that are still in it
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="playerName"></param>
        /// <returns></returns>
        Task<List<string>> LeaveGame(GameInfo gameInfo, string playerName);

        Task Reset();

        Task SendBroadcastMessage(CatanMessage message);
        Task SendPrivateMessage(string to, CatanMessage message);

        Task StartConnection(GameInfo info, string playerName);





        int UnprocessedMessages { get; set; }


        Task<bool> KeepAlive();
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
        Task UpgradeKnight(BuildingCtrl building);
        Task ActivateKnight(BuildingCtrl building, bool activate);
        Task MoveKnight(KnightCtrl Knight);
        BuildingState ValidateBuildingLocation(BuildingCtrl sender);

        bool HasEntitlement(Entitlement entitlement);
        Task DestroyCity(BuildingCtrl buildingCtrl);




    }

    public interface IGameController
    {
        bool IsCitiesAndKnights { get; }

        BuildingCtrl GetBuilding(int index);


        bool AutoRespondAndTheHuman { get; }
        CatanGames CatanGame { get; set; }

        GameState CurrentGameState { get; }

        PlayerModel CurrentPlayer { get; set; }
        PlayerModel LastPlayerToRoll { get; set; } //used in Supplemental builds
        PlayerModel NextPlayer { get; }

        List<int> CurrentRandomGoldTiles { get; }
        GameContainerCtrl GameContainer { get; }
        CatanGameData GameData { get; }
        List<int> HighlightedTiles { get; }

        bool IsServiceGame { get; }

        Log Log { get; }

        MainPageModel MainPageModel { get; }

        List<PlayerModel> PlayingPlayers { get; }
        bool MyTurn { get; }

        IRollLog RollLog { get; }
        PlayerModel TheHuman { get; }
        ICatanService Proxy { get; }
        bool BaronVisibility { get; }

        Task AddPlayer(string playerToAdd);

        Task ChangePlayer(ChangePlayerLog log);

        void CompleteRedo();

        void CompleteUndo();

        RandomBoardSettings CurrentRandomBoard();

        Task<bool> DetermineRollOrder(RollOrderLog log);
        Task ExecuteSynchronously(LogHeader logHeader, ActionType normal, MessageType messageType);
        RollState GetNextRollState();

        RandomBoardSettings GetRandomBoard();

        Task CreateGame(GameInfo gameInfo);

        PlayerModel NameToPlayer(string playerName);
        PlayerModel NameToPlayer(PlayerModel player);
        Task PickSettlementsAndRoads();
        Task<bool> PostMessage(LogHeader logHeader, ActionType actionType);

        DevCardType PurchaseNextDevCard();

        Task<bool> RedoAsync();

        void ResetAllBuildings();

        Task ResetRandomGoldTiles();

        Task ResetRollControl();

        (TradeResources Granted, TradeResources Baroned) ResourcesForRoll(PlayerModel player, RollModel roll, RollAction action);

        void SetHighlightedTiles(int roll);

        Task SetRandomBoard(RandomBoardLog randomBoard);

        Task SetRandomTileToGold(List<int> goldTiles);

        Task SetRoadState(UpdateRoadLog updateRoadModel);

        void SetSpyInfo(string sentBy, bool spyOn);

        Task SetState(SetStateLog log);

        void ShowRollsInPublicUi();

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

        Task UpdateBuilding(UpdateBuildingLog updateBuildingLog, ActionType action);

        Task UpdateKnight(KnightStateChangeLog knightStateChangeLog, ActionType actionType);
        Task MoveKnight(MoveKnightLog moveKnightLog, ActionType actionType);
        void AssignLargestArmy();

        Task HandlePirateRoll(RollModel rollModel, ActionType action);

        string PlayerListToCsv(List<PlayerModel> playersWithTooManyCards);
        void SetCurrentPlayer(PlayerModel playerModel);

        Task RolledSeven();
        Task ProtectCity(ProtectCityLog protectCityLog, ActionType normal);
        void SetBaronTile(TargetWeapon weapon, TileCtrl targetTile, bool showBaron);
        void MoveMerchant(Point to);
        PlayerModel PlayerFromId(Guid id);
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

        Task Replay(IGameController gameController);

    }

    public interface IMessageDeserializer
    {


        LogHeader Deserialize(string json);


    }

    public interface ITileControlCallback
    {


        void TileRightTapped(TileCtrl tile, RightTappedRoutedEventArgs rte);


    }

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
}