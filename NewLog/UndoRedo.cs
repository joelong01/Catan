using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    public class UndoRedoLog :  LogHeader, ILogController
    {
        
        public Guid LogEntryAffected { get; set; } // the ID I expect to Undo or redo
        public UndoRedoLog() : base()
        {
            
        }
        public override string ToString()
        {
            return $"[Action={Action}][CreatedBy={CreatedBy}][Id={LogEntryAffected}]";
        }
        public static Task UdoRedo(IGameController gameController, CatanAction action)
        {
            if (!(action == CatanAction.Undo || action == CatanAction.Redo))
                return Task.CompletedTask;

            UndoRedoLog log = new UndoRedoLog()
            {
                Action = action,
                LogEntryAffected = action == CatanAction.Undo ? gameController.Log.PeekAction.LogId : gameController.Log.PeekUndo.LogId
            };

           return gameController.Log.PushAction(log);


        }
        public Task Do(IGameController gameController, LogHeader logHeader)
        {
            throw new NotImplementedException();
        }

        public Task Redo(IGameController gameController, LogHeader logHeader)
        {
            throw new NotImplementedException();
        }

        public Task Undo(IGameController gameController, LogHeader logHeader)
        {
            throw new NotImplementedException();
        }
    }
}
