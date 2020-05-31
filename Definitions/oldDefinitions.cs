using System.Collections.Generic;

using Catan.Proxy;

using Windows.Storage;

namespace Catan10
{
    public enum LayoutDirection { ClockWise, CounterClockwise };

    public class ChangeGameEventArgs
    {
        #region Constructors

        public ChangeGameEventArgs(StorageFile f)
        {
            File = f;
        }

        #endregion Constructors

        #region Properties

        public StorageFile File { get; set; }

        #endregion Properties
    }

    public class LogStateTranstion
    {
        #region Constructors

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

        #endregion Constructors

        #region Properties

        public GameState NewState { get; set; } = GameState.Uninitialized;
        public GameState OldState { get; set; } = GameState.Uninitialized;
        public List<int> RandomGoldTiles { get; set; } = new List<int>();

        #endregion Properties

        #region Methods

        public static LogStateTranstion Deserialize(string json)
        {
            return CatanProxy.Deserialize<LogStateTranstion>(json);
        }

        public override string ToString()
        {
            return CatanProxy.Serialize(this);
        }

        #endregion Methods
    }

    public class UndoEventArgs
    {
        #region Properties

        public UndoOrder UndoOrder { get; set; } = UndoOrder.None;

        #endregion Properties
    }
}
