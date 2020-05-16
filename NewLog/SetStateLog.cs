using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    public class SetStateLog : LogHeader, ILogController
    {

        

        public SetStateLog() : base() { }
        public List<int> RandomGoldTiles { get; set; } = new List<int>();

        public static async Task<SetStateLog> SetState(IGameController gameController, GameState newState)
        {
            SetStateLog log = new SetStateLog()
            {
                 Action = CatanAction.ChangedState,
                 NewState = newState, 
                 RandomGoldTiles = gameController.CurrentRandomGoldTiles

            };

            await gameController.SetState(log);
            return log;

        }
        public Task Do(IGameController gameController, LogHeader logHeader)
        {
            return gameController.SetState(logHeader as SetStateLog);
            
        }

        public Task Redo(IGameController gameController, LogHeader logHeader)
        {
            return gameController.SetState(logHeader as SetStateLog);
        }

        public Task Undo(IGameController gameController, LogHeader logHeader)
        {
            return gameController.UndoSetState(logHeader as SetStateLog);
        }

        public override string ToString()
        {
            return $"{PlayerName}: [OldState={OldState}][NewState={NewState}]";
        }
    }
}
