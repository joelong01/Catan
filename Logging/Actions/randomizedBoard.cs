using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class RandomBoardLog : LogHeader, ILogController
    {
        #region Constructors

        public RandomBoardLog() : base()
        {
        }

        #endregion Constructors

        #region Properties

        public int GameIndex { get; set; }
        public RandomBoardSettings NewRandomBoard { get; set; }
        public RandomBoardSettings PreviousRandomBoard { get; set; }

        #endregion Properties

        #region Methods

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

        /// <summary>
        ///     can't change state!
        /// </summary>
        /// <param name="gameController"></param>
        /// <param name="gameIndex"></param>
        /// <returns></returns>
        public static async Task RandomizeBoard(IGameController gameController, int gameIndex)
        {
            
            RandomBoardLog logHeader = new RandomBoardLog()
            {
                Action = CatanAction.RandomizeBoard,
                CanUndo = true, // gameController.Log.PeekAction.NewState == GameState.PickingBoard,
                NewState = GameState.PickingBoard,
                NewRandomBoard = gameController.GetRandomBoard(),
                PreviousRandomBoard = gameController.CurrentRandomBoard(),
                GameIndex = gameIndex
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
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
            return $"[Action={Action}][[SentBy={SentBy}]]";
        }

        //
        //  if transition out of PickingBoard, will turn tiles facedown
        public Task Undo(IGameController gameController)
        {
            return gameController.UndoSetRandomBoard(this);
        }

        #endregion Methods
    }
}
