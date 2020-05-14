using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        public int RollCount { get; set; } = 0;
        [JsonIgnore]
        public int Roll => DiceOne + DiceTwo;

        public static async Task<SynchronizedRollLog> StartSyncronizedRoll(IGameController gameController, int dice1, int dice2)
        {
            SynchronizedRollLog log = new SynchronizedRollLog()
            {
                CanUndo = false,
                Action = CatanAction.RollToSeeWhoGoesFirst,
                NewState = GameState.WaitingForRollForOrder,
                DiceOne = dice1,
                DiceTwo = dice2,
                RollCount = gameController.CurrentPlayer.GameData.SyncronizedPlayerRolls.Rolls.Count, // this works because until we start the game, CurrentPlayer == TheHuman
            };
            await gameController.SynchronizedRoll(log);
            return log;
        }
        public Task Do(IGameController gameController, LogHeader logHeader)
        {
            SynchronizedRollLog log = logHeader as SynchronizedRollLog;
            return gameController.SynchronizedRoll(log);
        }

        public Task Redo(IGameController gameController, LogHeader logHeader)
        {
            SynchronizedRollLog log = logHeader as SynchronizedRollLog;
            return gameController.SynchronizedRoll(log);
        }

        public Task Undo(IGameController gameController, LogHeader logHeader)
        {
            throw new NotImplementedException();
        }
    }
}
