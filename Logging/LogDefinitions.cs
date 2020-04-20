using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Catan10
{
    public enum LogState
    {
        Normal,
        Replay,
        Undo
    }

    public interface ILogParserHelper
    {
        TileCtrl GetTile(int tileIndex, int gameIndex);
        RoadCtrl GetRoad(int roadIndex, int gameIndex);
        BuildingCtrl GetBuilding(int buildingIndex, int gameIndex);
        PlayerModel GetPlayerData(int playerIndex);
    }

    public interface ILog
    {
        Task AddLogEntry(PlayerModel player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0);
        void PostLogEntry(PlayerModel player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0);
    }

    public enum LogType { Normal, Undo, Replay, DoNotLog, DoNotUndo };



    public delegate void RedoPossibleHandler(bool redo);
    public delegate void StateChangedHandler(GameState oldState, GameState newState);
}