using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    public  class MoveKnightLog : LogHeader, ILogController
    {
        public int Index { get; set; }
        public KnightRank KnightRank { get; set; }


        public static async Task PostLog(IGameController gameController, KnightCtrl knight)
        {
            MoveKnightLog logHeader = new MoveKnightLog()
            {
                Index = knight.BuildingIndex,
                KnightRank = knight.KnightRank
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
           return gameController.MoveKnight(this, ActionType.Normal);
        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Replay(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.MoveKnight(this, ActionType.Undo);
        }
    }
}
