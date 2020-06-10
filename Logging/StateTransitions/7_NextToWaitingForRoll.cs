using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     1. move the player
    ///     2. set the random gold tiles
    ///     3. reset ResourcesThisTurn
    ///     4. Reset and Show the roll UI
    /// </summary>
    public class WaitingForNextToWaitingForRoll : LogHeader, ILogController
    {
        public TradeResources ResourcesThisTurn { get; set; }
        public RollState RollState { get; set; }

        public static async Task PostLog(IGameController gameController)
        {
            var rollState = gameController.GetNextRollState();
            WaitingForNextToWaitingForRoll logHeader = new WaitingForNextToWaitingForRoll()
            {
                CanUndo = true,
                RollState = rollState,
                ResourcesThisTurn = gameController.CurrentPlayer.GameData.Resources.ResourcesThisTurn,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForRoll,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            //
            //  remember the log is already written!

            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRoll);
            ChangePlayerHelper.ChangePlayer(gameController, 1);
            Debug.Assert(this.RollState != null);
            await gameController.ResetRollControl();

            await gameController.PushRollState(this.RollState);
            //
            // if we have a roll for this turn already, use it.
            if (this.RollState.Rolls != null && this.RollState.Rolls.Count > 0)
            {
                await WaitingForRollToWaitingForNext.PostRollMessage(gameController, gameController.RollLog.NextRolls);
            }
        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            ChangePlayerHelper.ChangePlayer(gameController, -1);

            //
            //  hide the rolls in the public data control
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.Resources.ResourcesThisTurn = new TradeResources();
            });

            //
            // if we have a roll for this turn already, use it.
            if (gameController.RollLog.CanRedo)
            {
                await WaitingForRollToWaitingForNext.PostRollMessage(gameController, gameController.RollLog.NextRolls);
            }
        }
    }
}