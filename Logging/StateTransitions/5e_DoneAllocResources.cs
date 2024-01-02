using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     UI Pause
    ///     wipes the resources for the turn
    /// </summary>
    public class AllocateResourcesReverseToDoneAllocResources : LogHeader, ILogController
    {
        internal static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.AllocateResourceReverse);

            AllocateResourcesReverseToDoneAllocResources logHeader = new AllocateResourcesReverseToDoneAllocResources()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.AllocateResourceReverse,
                NewState = GameState.DoneResourceAllocation,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public AllocateResourcesReverseToDoneAllocResources()
        {
        }

        /// <summary>
        ///    pauses for UI continue.  leave the resources so people can remember what was picked
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>

        public async Task Do(IGameController gameController)
        {
            Debug.Assert(gameController.CurrentGameState == GameState.DoneResourceAllocation);


            // show all the resources the players got during the allocation phase


            foreach (var p in gameController.PlayingPlayers)
            {


                var tr =  p.GameData.Resources.TotalResourcesForGame;
                p.GameData.Resources.GrantResources(tr.GetNegated(), true);
                p.GameData.Resources.ResourcesThisTurn.Reset(); 
                Debug.Assert(p.GameData.Resources.TotalResourcesCollection.ResourceCount == 0);
                p.GameData.Resources.GrantResources(tr, true);
                await Task.Delay(10);

            }

        }



        public Task Replay(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Redo(IGameController gameController)
        {
            await Task.Delay(0);
        }

        public async Task Undo(IGameController gameController)
        {
            await Task.Delay(0);
        }
    }
}