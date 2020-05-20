using System.Diagnostics.Contracts;
using System.Threading.Tasks;

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

        public static async Task<RandomBoardLog> RandomizeBoard(IGameController gameController, int gameIndex)
        {
            RandomBoardLog model = new RandomBoardLog()
            {
                NewState = GameState.PickingBoard,
                NewRandomBoard = gameController.GetRandomBoard(),
                PreviousRandomBoard = gameController.CurrentRandomBoard(),
                GameIndex = gameIndex
            };
            await gameController.Log.PushAction(model);
            return model;
        }

        public Task Do(IGameController gameController, LogHeader logHeader)
        {
            return gameController.SetRandomBoard(logHeader as RandomBoardLog);
        }

        public Task Redo(IGameController gameController, LogHeader logHeader)
        {
            return gameController.SetRandomBoard(logHeader as RandomBoardLog);
        }

        // this to just double check
        public override string ToString()
        {
            return $"[Action={Action}][CreatedBy={CreatedBy}]";
        }

        public Task Undo(IGameController gameController, LogHeader logHeader)
        {
            return gameController.UndoSetRandomBoard(logHeader as RandomBoardLog);
        }
    }
}