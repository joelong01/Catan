using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///
    ///    We do this state transition when somebody
    ///    1. clicks on "Start" after allocation
    ///    2. Rolls
    ///    3. any number of arbitrary actions
    ///    4. does Undo past their roll back to the "Start" state ("DoneAllocateResources")
    ///    5. clicks *next* (not Redo)
    ///
    /// we need to make sure they get the same roll and the same gold tiles they got the first time.
    ///
    /// </summary>
    public class DoneAllocResourcesToWaitingForNext : LogHeader, ILogController
    {
        #region Methods

        public static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.DoneResourceAllocation);
            Contract.Assert(gameController.Log.RollLog.CanRedo, "you have to be able to redo a roll log to be in this state");

            DoneAllocResourcesToWaitingForNext logHeader = new DoneAllocResourcesToWaitingForNext()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.DoneResourceAllocation,
                NewState = GameState.WaitingForNext,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            await gameController.Log.RollLog.RedoRoll();
            if (gameController.Log.RollLog.LastRoll == 7)
            {
                await WaitingForNextToMustRollBaron.PostLog(gameController);
            }

            //
            //  hide the rolls in the public data control
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.RollOrientation = TileOrientation.FaceDown;
                p.GameData.Resources.ResourcesThisTurn = new TradeResources();
                p.GameData.KnightEligible = false;
            });

            gameController.CurrentPlayer.GameData.KnightEligible = (gameController.CurrentPlayer.GameData.Resources.UnplayedKnights > 0);
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.Log.RollLog.UndoRoll();
        }

        #endregion Methods
    }
}
