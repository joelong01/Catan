using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class MustMoveBaronLog : LogHeader, ILogController
    {
        public MoveBaronReason Reason { get; set; }
        public GameState StartingState { get; set; }

        public static async Task PostLog(IGameController gameController, MoveBaronReason reason)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForNext ||
                            gameController.CurrentGameState == GameState.WaitingForRoll);

            if (reason == MoveBaronReason.Rolled7)
            {
                Contract.Assert(gameController.CurrentGameState == GameState.WaitingForNext);
            }

            MustMoveBaronLog logHeader = new MustMoveBaronLog()
            {
                NewState = GameState.MustMoveBaron,
                CanUndo = true,
                Reason = reason,
                StartingState = gameController.CurrentGameState,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            //
            //   nothing to do -- we just want the state and state message set

            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }
    }
}
