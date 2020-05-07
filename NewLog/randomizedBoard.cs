using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    public class RandomBoardLog : LogHeader, ILogController
    {
        public RandomBoardSettings NewRandomBoard { get; set; }
        public RandomBoardSettings PreviousRandomBoard { get; set; }
        public int GameIndex { get; set; } // this to just double check
        public RandomBoardLog() : base()
        {
            Action = CatanAction.RandomizeBoard;
        }

        public static async Task<RandomBoardLog> RandomizeBoard(IGameController gameController, int gameIndex)
        {
            RandomBoardLog model = new RandomBoardLog()
            {                
                NewRandomBoard = gameController.GetRandomBoard(),
                PreviousRandomBoard = gameController.CurrentRandomBoard(),
                GameIndex = gameIndex
            };
            await gameController.SetRandomBoard(model);
            return model;

        }

        public Task Do(IGameController gameController, LogHeader logHeader)
        {
            return gameController.SetRandomBoard(logHeader as RandomBoardLog);
        }

        public Task Undo(IGameController gameController, LogHeader logHeader)
        {

            return gameController.UndoSetRandomBoard(logHeader as RandomBoardLog);
        }

        public Task Redo(IGameController gameController, LogHeader logHeader)
        {
            return gameController.SetRandomBoard(logHeader as RandomBoardLog);
        }

    }
}

