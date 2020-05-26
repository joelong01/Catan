using System.Collections.Generic;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class SetStateLog : LogHeader, ILogController
    {
        public SetStateLog() : base()
        {
        }

        public List<int> RandomGoldTiles { get; set; } = new List<int>();

        public static async Task SetState(IGameController gameController, GameState newState)
        {
            GameState oldState = gameController.CurrentGameState;
            bool canUndo = true;
            if (oldState == GameState.WaitingForRollForOrder || oldState == GameState.WaitingForRoll)
            {
                canUndo = false;
            }
            SetStateLog logHeader = new SetStateLog()
            {
                CanUndo = canUndo,
                Action = CatanAction.ChangedState,
                OldState = oldState,
                NewState = newState,
                RandomGoldTiles = gameController.CurrentRandomGoldTiles
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return gameController.SetState(this);
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.SetState(this);
        }

        public override string ToString()
        {
            return $"[Action={Action}][SentBy={SentBy}][OldState={OldState}][NewState={NewState}]";
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.UndoSetState(this);
        }
    }
}