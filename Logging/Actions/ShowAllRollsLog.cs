using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class ShowAllRollsLog : LogHeader, ILogController
    {
        public List<RollModel> Rolls { get; set; } = new List<RollModel>();

        public ShowAllRollsLog() : base()
        {
            Action = CatanAction.Rolled;
        }

        public static async Task Post(IGameController gameController, List<RollModel> rolls)
        {
            ShowAllRollsLog logHeader = new ShowAllRollsLog()
            {
                CanUndo = false,
                LogType = LogType.DoNotLog,
                Action = CatanAction.ShowAllRolls,
                Rolls = rolls,
                SentBy = gameController.TheHuman
            };
            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        /// <summary>
        ///     This just sets the orientation of all the rolls to faceup on all the machines
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public Task Do(IGameController gameController)
        {
            PlayerModel sentBy = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(sentBy != null);
            sentBy.GameData.SyncronizedPlayerRolls.LatestRolls.ForEach((r) => r.Orientation = TileOrientation.FaceUp);            
            
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