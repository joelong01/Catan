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


    public class ShowAllRollsLog : LogHeader, ILogController
    {
        public ShowAllRollsLog() : base()
        {
            Action = CatanAction.Rolled;
        }

        public List<RollModel> Rolls { get; set; } = new List<RollModel>();


        public static async Task Post(IGameController gameController, List<RollModel> rolls)
        {
            ShowAllRollsLog logHeader = new ShowAllRollsLog()
            {
                CanUndo = false,
                LogType= LogType.DoNotLog,
                Action = CatanAction.RollToSeeWhoGoesFirst,
                NewState = GameState.WaitingForRollForOrder,
                Rolls = rolls,
                SentBy = gameController.TheHuman.PlayerName

            };
            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {

            PlayerModel sentBy = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(sentBy != null);
            RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRolls(this.Rolls);
            Contract.Assert(pickedRoll != null);
            Contract.Assert(pickedRoll.DiceOne > 0 && pickedRoll.DiceOne < 7);
            Contract.Assert(pickedRoll.DiceTwo > 0 && pickedRoll.DiceTwo < 7);

            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
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


