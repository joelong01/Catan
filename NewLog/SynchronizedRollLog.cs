using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{


    public class SynchronizedRollLog : LogHeader, ILogController
    {
        public SynchronizedRollLog() : base()
        {
            Action = CatanAction.Rolled;
        }

        public List<RollModel> Rolls { get; set; } = new List<RollModel>();


        public static async Task StartSyncronizedRoll(IGameController gameController, List<RollModel> rolls)
        {
            SynchronizedRollLog logHeader = new SynchronizedRollLog()
            {
                CanUndo = false,
                Action = CatanAction.RollToSeeWhoGoesFirst,
                NewState = GameState.WaitingForRollForOrder,
                Rolls = rolls,
                SentBy = gameController.TheHuman.PlayerName

            };
            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            gameController.ShowRollsInPublicUi();
            return gameController.SynchronizedRoll(this);
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.SynchronizedRoll(this);
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
