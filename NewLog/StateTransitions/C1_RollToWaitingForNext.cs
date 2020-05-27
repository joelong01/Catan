
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Unlike previous strategies, this records only the roll and relies on the state of the game to calculate the consequences of that roll.
    ///     1. get and store the random gold tiles
    /// </summary>
    public class WaitingForRollToWaitingForNext : LogHeader, ILogController
    {
        
        internal static async Task PostLog(IGameController gameController)
        {

            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRoll);

            WaitingForRollToWaitingForNext logHeader = new WaitingForRollToWaitingForNext()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.WaitingForNext,
                NewState = GameState.WaitingForRoll,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }

        public Task Redo(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }
    }
}
