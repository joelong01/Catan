using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Catan10
{

    class GoldStack
    {
        private Stack<List<int>> Done = new Stack<List<int>>();
        private Stack<List<int>> UnDone = new Stack<List<int>>();
        public void Add(List<int> goldTileIndices)
        {
            Done.Push(goldTileIndices);
        }

        public List<int> GetUndoneRoll()
        {
            if (UnDone.Count == 0)
            {
                return null;
            }
            else
            {
                return UnDone.Pop();
            }
        }

        public void UndoLast()
        {
            if (Done.Count > 0)
            {

                UnDone.Push(Done.Pop());
            }
        }

        public List<int> Peek()
        {
            if (Done.Count == 0) return null;
            return Done.Peek();
        }

        public List<int> PopDone()
        {
            if (Done.Count == 0) return null;
            return Done.Pop();
        }
    }

    internal class ToPickGold : LogHeader, ILogController
    {
        static GoldStack GoldStack { get; set; } = new GoldStack();
        List<int> GoldTiles { get; set; }
        public static async Task PostLog(IGameController gameController, int goldTileCount)
        {
            if (goldTileCount == 0) return;

            var tiles = GoldStack.GetUndoneRoll();
            if (tiles == null)
            {
                tiles = gameController.GameContainer.PickRandomTilesToBeGold(goldTileCount);
            }

            var logEntry = new ToPickGold()
            {
                NewState = GameState.WaitingForRoll,
                GoldTiles = tiles,
                CanUndo = true

            };

            await gameController.PostMessage(logEntry, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            await gameController.GameContainer.SetRandomTilesToGold(GoldTiles);
            GoldStack.Add(this.GoldTiles);

    
        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Replay(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            GoldStack.UndoLast();
            await gameController.GameContainer.ResetRandomGoldTiles();

            // peek into the Done stack and set the random gold to what we find there
            var gold = GoldStack.Peek();
            await gameController.GameContainer.SetRandomTilesToGold(gold);
        }
    }
}
