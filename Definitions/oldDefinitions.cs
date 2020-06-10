using System.Collections.Generic;

using Catan.Proxy;

using Windows.Storage;

namespace Catan10
{
    public enum LayoutDirection { ClockWise, CounterClockwise };

    public class ChangeGameEventArgs
    {
        public StorageFile File { get; set; }

        public ChangeGameEventArgs(StorageFile f)
        {
            File = f;
        }
    }

    public class LogStateTranstion
    {
        public GameState NewState { get; set; } = GameState.Uninitialized;

        public GameState OldState { get; set; } = GameState.Uninitialized;

        public List<int> RandomGoldTiles { get; set; } = new List<int>();

        public LogStateTranstion()
        {
        }

        public LogStateTranstion(GameState old, GameState newState)
        {
            OldState = old;
            NewState = newState;
        }

        public LogStateTranstion(string saved)
        {
            Deserialize(saved);
        }

        public static LogStateTranstion Deserialize(string json)
        {
            return CatanProxy.Deserialize<LogStateTranstion>(json);
        }

        public override string ToString()
        {
            return CatanProxy.Serialize(this);
        }
    }

    public class UndoEventArgs
    {
        public UndoOrder UndoOrder { get; set; } = UndoOrder.None;
    }
}
