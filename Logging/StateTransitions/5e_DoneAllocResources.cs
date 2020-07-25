﻿using System.Diagnostics.Contracts;
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

        public Task Do(IGameController gameController)
        {
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.Resources.ResourcesThisTurn.Reset();
                p.GameData.Resources.ResourcesThisTurn.AddResources(p.GameData.Resources.Current);
            });
            return Task.CompletedTask;
        }
        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Redo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }
    }
}