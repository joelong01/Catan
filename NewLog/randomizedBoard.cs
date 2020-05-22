using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class RandomBoardLog : LogHeader, ILogController
    {
        public RandomBoardLog() : base()
        {
            Action = CatanAction.RandomizeBoard;
        }

        public int GameIndex { get; set; }
        public RandomBoardSettings NewRandomBoard { get; set; }
        public RandomBoardSettings PreviousRandomBoard { get; set; }

        public static Task LogRedoAction(IGameController gameController)
        {
            var log = gameController.Log;
            var logRecord = log.PeekUndo;
            Contract.Assert(logRecord.GetType() == typeof(RandomBoardLog));

            return log.Redo(logRecord);
        }

        public static Task LogUndoAction(IGameController gameController)
        {
            var log = gameController.Log;
            var logRecord = log.PeekAction;
            Contract.Assert(logRecord.GetType() == typeof(RandomBoardLog));

            return log.Undo(logRecord);
        }

        public static async Task RandomizeBoard(IGameController gameController, int gameIndex)
        {
            RandomBoardLog logHeader = new RandomBoardLog()
            {
                NewState = GameState.PickingBoard,
                NewRandomBoard = gameController.GetRandomBoard(),
                PreviousRandomBoard = gameController.CurrentRandomBoard(),
                GameIndex = gameIndex
            };
           
            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
           
        }

        public Task Do(IGameController gameController)
        {
            return gameController.SetRandomBoard(this);
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.SetRandomBoard(this);
        }

        // this to just double check
        public override string ToString()
        {
            return $"[Action={Action}][CreatedBy={CreatedBy}]";
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.UndoSetRandomBoard(this);
        }
    }
}