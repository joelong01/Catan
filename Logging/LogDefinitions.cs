﻿using Catan.Proxy;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Catan10
{
    public enum LogState
    {
        Normal,
        Replay,
        Undo,
        ServiceUpdate
    }

    public interface ILogParserHelper
    {
        TileCtrl GetTile(int tileIndex);
        RoadCtrl GetRoad(int roadIndex);
        BuildingCtrl GetBuilding(int buildingIndex);
        PlayerModel GetPlayerData(int playerIndex);
    }

    public interface ILog
    {
        Task AddLogEntry(PlayerModel player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0);
        void PostLogEntry(PlayerModel player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0);
    }

    public enum LogType { Normal, Undo, Replay, DoNotLog, DoNotUndo, Redo };
    public delegate void RedoPossibleHandler(bool redo);
    public delegate void StateChangedHandler(GameState oldState, GameState newState);

}