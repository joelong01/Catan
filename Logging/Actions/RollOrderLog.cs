using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class RollOrderLog : LogHeader, ILogController
    {
        public List<RollModel> Rolls { get; set; } = new List<RollModel>();

        public RollOrderLog() : base()
        {
        }

        public static async Task PostMessage(IGameController gameController, List<RollModel> rolls)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRollForOrder);

            RollOrderLog logHeader = new RollOrderLog()
            {
                CanUndo = false,
                Action = CatanAction.Rolled,
                Rolls = rolls,
                SentBy = gameController.TheHuman
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            gameController.ShowRollsInPublicUi();
            bool finished = await gameController.DetermineRollOrder(this);

            if (finished && gameController.TheHuman == gameController.CurrentPlayer) // whoever wins sends the message saying we have finished rolling for order
            {
                await RollOrderFinalizedLog.PostLog(gameController, gameController.PlayingPlayers);
            }
        }

        /// <summary>
        ///     We don't need to worry about this message as after all these messages happen, an order is set and an explicit message is sent to set the order.
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public Task Replay (IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.DetermineRollOrder(this);
        }

        public override string ToString()
        {
            return $"[DiceOne={Rolls[0].DiceOne}][DiceTwo={Rolls[0].DiceTwo}]" + base.ToString();
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }
    }
}