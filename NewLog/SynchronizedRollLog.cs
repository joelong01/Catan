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
        public SynchronizedRollLog(): base() 
        {
            Action = CatanAction.Rolled;
        }

        public int DiceOne { get; set; } = -1;

        public int DiceTwo { get; set; } = -1;

        [JsonIgnore]
        public int Roll => DiceOne + DiceTwo;

        public static async Task StartSyncronizedRoll(IGameController gameController, int dice1, int dice2)
        {
            SynchronizedRollLog logHeader = new SynchronizedRollLog()
            {
                CanUndo = false,
                Action = CatanAction.RollToSeeWhoGoesFirst,
                NewState = GameState.WaitingForRollForOrder,
                DiceOne = dice1,
                DiceTwo = dice2,

            };
            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return gameController.SynchronizedRoll(this);
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.SynchronizedRoll(this);
        }

        public override string ToString()
        {
            return $"[Action={Action}][CreatedBy={CreatedBy}][DiceOne={DiceOne}][DiceTwo={DiceTwo}]";
        }
        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }
    }
}
