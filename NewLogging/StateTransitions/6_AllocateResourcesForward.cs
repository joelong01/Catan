using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{

    public static class AllocationPhaseHelper
    {
        public static void ChangePlayer(IGameController gameController, int numberofPositions)
        {
            Contract.Assert(gameController.CurrentPlayer != null);

            List<PlayerModel> playingPlayers = gameController.PlayingPlayers;

            int idx = playingPlayers.IndexOf(gameController.CurrentPlayer);

            Contract.Assert(idx != -1, "The player needs to be playing!");

            idx += numberofPositions;
            int count = playingPlayers.Count;
            if (idx >= count) idx -= count;
            if (idx < 0) idx += count;

            var newPlayer = playingPlayers[idx];
            gameController.CurrentPlayer = newPlayer;

        }

        public static void GrantEntitlements(IGameController gameController, string to)
        {
            var player = gameController.NameToPlayer(to);
            player.GameData.Resources.GrantEntitlement(Entitlement.Road);
            player.GameData.Resources.GrantEntitlement(Entitlement.Settlement);
        }

        public static void RevokeEntitlements(IGameController gameController, string to)
        {
            var player = gameController.NameToPlayer(to);
            player.GameData.Resources.RevokeEntitlement(Entitlement.Road);
            player.GameData.Resources.RevokeEntitlement(Entitlement.Settlement);
        }
        public static void GrantResources(IGameController gameController, BuildingCtrl building, string to)
        {
            var player = gameController.NameToPlayer(to);
            TradeResources tr = new TradeResources();
            foreach (var kvp in building.BuildingToTileDictionary)
            {
                tr.Add(kvp.Value.ResourceType, 1);
            }
            player.GameData.Resources.GrantResources(tr);
        }

        public static void RevokeResources(IGameController gameController, BuildingCtrl building, string to)
        {
            var player = gameController.NameToPlayer(to);
            TradeResources tr = new TradeResources();
            foreach (var kvp in building.BuildingToTileDictionary)
            {
                tr.Add(kvp.Value.ResourceType, 1);
            }

            player.GameData.Resources.GrantResources(tr.GetNegated());
        }       
    }
    /// <summary>
    ///     Allocate one road and one settlement to the player
    ///     Reset the roll control for next time
    /// </summary>
    public class BeginAllocationToAllocateResourcesForward : LogHeader, ILogController
    {
        internal static async Task PostLog(IGameController gameController)
        {
            //
            //  Player Order and Current players should be consistent
            //  Verify the players are consistent across the machines
            await VerifyPlayers.PostMessage(gameController);

            Contract.Assert(gameController.CurrentGameState == GameState.BeginResourceAllocation);

            BeginAllocationToAllocateResourcesForward logHeader = new BeginAllocationToAllocateResourcesForward()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,                
                NewState = GameState.AllocateResourceForward,
            };

            Contract.Assert(logHeader.OldState == GameState.BeginResourceAllocation);

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }
        public Task Do(IGameController gameController)
        {
            
            gameController.HideRollsInPublicUi();
            
            AllocationPhaseHelper.GrantEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
            return Task.CompletedTask;

        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            AllocationPhaseHelper.RevokeEntitlements(gameController, gameController.CurrentPlayer.PlayerName);                        
            return Task.CompletedTask;

        }
    }
}