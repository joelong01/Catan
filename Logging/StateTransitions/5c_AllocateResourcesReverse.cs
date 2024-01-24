﻿using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Grant/Revoke (Do/Undo) one settlement and one road
    /// </summary>
    public class AllocateResourcesForwardToAllocateResourcesReverse : LogHeader, ILogController
    {
        internal static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.AllocateResourceForward);

            AllocateResourcesForwardToAllocateResourcesReverse logHeader = new AllocateResourcesForwardToAllocateResourcesReverse()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.AllocateResourceForward,
                NewState = GameState.AllocateResourceReverse,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public AllocateResourcesForwardToAllocateResourcesReverse()
        {
        }

        public async Task Do(IGameController gameController)
        {
            AllocationPhaseHelper.GrantEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
            if (gameController.CurrentPlayer == gameController.TheHuman && gameController.MainPageModel.Settings.AutoRespond)
            {
                await gameController.PickSettlementsAndRoads();
            }
            
        }
        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            AllocationPhaseHelper.RevokeEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
             await DefaultTask;
        }
    }
}