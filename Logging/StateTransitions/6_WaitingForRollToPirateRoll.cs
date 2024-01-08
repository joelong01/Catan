using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Catan10
{
    internal class WaitingForRollToPirateRoll : LogHeader, ILogController
    {
        public RollModel RollModel { get; set; }
        internal static async Task PostRollMessage(IGameController gameController, RollModel roll)
        {
            //
            //  if it isn't a pirate dice, just take care of the roll
            if (roll.SpecialDice != SpecialDice.Pirate)
            {
                await WaitingForRollToWaitingForNext.PostRollMessage(gameController, roll);
                return;
            }

            var logEntry = new WaitingForRollToPirateRoll()
            {
                NewState = GameState.HandlePirates,
                RollModel = roll
            };

            await gameController.PostMessage(logEntry, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            await gameController.HandlePirateRoll(RollModel, ActionType.Normal);
        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public Task Replay(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public async Task Undo(IGameController gameController)
        {
            await gameController.HandlePirateRoll(RollModel, ActionType.Undo);
        }
    }
}
