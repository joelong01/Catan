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

    public class WaitingForStartToAllocateResourcesForward : LogHeader, ILogController
    {
        internal static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForStart);

            WaitingForStartToAllocateResourcesForward logHeader = new WaitingForStartToAllocateResourcesForward()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.WaitingForStart,
                NewState = GameState.AllocateResourceForward,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }
        public Task Do(IGameController gameController)
        {
            gameController.MainPageModel.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.RollOrientation = TileOrientation.FaceDown;
                p.GameData.SyncronizedPlayerRolls.DiceOne = 0;
                p.GameData.SyncronizedPlayerRolls.DiceTwo = 0;
            });

            AllocationPhaseHelper.ChangePlayer(gameController, 0);
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