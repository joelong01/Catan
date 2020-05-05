using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    public class RandomBoardModel : LogHeader
    {
        public RandomBoardSettings RandomBoard { get; set; }
        public int GameIndex { get; set; } // this to just double check
        public RandomBoardModel() : base()
        {
            Action = CatanAction.RandomizeBoard;
        }
    }


    public static class RandomBoardController
    {

        public static async Task<RandomBoardModel> RandomizeBoard(IGameController gameController, int gameIndex)
        {
            RandomBoardModel model = new RandomBoardModel()
            {
                PlayerName = gameController.CurrentPlayer?.PlayerName,
                RandomBoard = gameController.GetRandomBoard(),
                GameIndex = gameIndex
            };
            await gameController.SetRandomBoard(model.RandomBoard);
            return model;
        }

        public static async Task Redo(IGameController gameController, RandomBoardModel model)
        {
            await gameController.SetRandomBoard(model.RandomBoard);
        }
    }
}

