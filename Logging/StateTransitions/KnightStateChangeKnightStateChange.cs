using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    public class KnightStateChangeLog: LogHeader, ILogController
    {
        public int BuildingIndex { get; set; } = -1;
        public KnightRank NewRank { get; set; } = KnightRank.Unset;
        public KnightRank OldRank { get; set; }
        public bool OldActivated { get; set; }
        public bool NewActivated { get; set; }
        public static async Task ToggleActiveState(IGameController gameController, int index, KnightCtrl knight, KnightRank newRank, bool activated)
        {
           
            var logHeader = new KnightStateChangeLog
            {
                BuildingIndex= index,
                NewRank= newRank,
                OldRank = knight.KnightRank,
                OldActivated= knight.Activated,
                NewActivated= activated

            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return gameController.UpdateKnight(this, ActionType.Normal);
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.UpdateKnight(this, ActionType.Redo);
        }

        public Task Replay(IGameController gameController)
        {
            return gameController.UpdateKnight(this, ActionType.Replay);
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.UpdateKnight(this, ActionType.Undo);
        }
    }
}
