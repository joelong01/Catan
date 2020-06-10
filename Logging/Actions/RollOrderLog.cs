using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class RollOrderLog : LogHeader, ILogController
    {
        #region Constructors

        public RollOrderLog() : base()
        {
        }

        #endregion Constructors

        #region Properties

        public List<RollModel> Rolls { get; set; } = new List<RollModel>();

        #endregion Properties

        #region Methods

        public static async Task PostMessage(IGameController gameController, List<RollModel> rolls)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRollForOrder);

            RollOrderLog logHeader = new RollOrderLog()
            {
                CanUndo = false,
                Action = CatanAction.Rolled,
                Rolls = rolls,
                SentBy = gameController.TheHuman.PlayerName
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            gameController.ShowRollsInPublicUi();
            bool finished = await gameController.DetermineRollOrder(this);
            if (finished)
            {
                await WaitingForRollOrderToBeginResourceAllocation.PostLog(gameController);
            }
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

        #endregion Methods
    }
}
