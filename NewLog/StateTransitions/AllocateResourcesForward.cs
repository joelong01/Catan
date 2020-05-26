using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
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

        public static Task DoAllocateForwardResources(IGameController gameController, string sentByName)
        {
            gameController.MainPageModel.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.RollOrientation = TileOrientation.FaceDown;
                p.GameData.SyncronizedPlayerRolls.DiceOne = 0;
                p.GameData.SyncronizedPlayerRolls.DiceTwo = 0;
            });
            //
            //  we only get one of these, so we need to give whoever is the first player the resources

            var firstPlayer = gameController.MainPageModel.PlayingPlayers[0];

            //
            //  during allocation phase, you get one road and one settlement
            firstPlayer.GameData.Resources.GrantEntitlement(Entitlement.Road);
            firstPlayer.GameData.Resources.GrantEntitlement(Entitlement.Settlement);

            ChangePlayer(gameController, 1);

            return Task.CompletedTask;
        }

        public static Task DoAllocateReverseResources(IGameController gameController, string sentByName)
        {
            var sentBy = gameController.NameToPlayer(sentByName);
            gameController.MainPageModel.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.RollOrientation = TileOrientation.FaceDown;
                p.GameData.SyncronizedPlayerRolls.DiceOne = 0;
                p.GameData.SyncronizedPlayerRolls.DiceTwo = 0;
            });

            //
            //  during allocation phase, you get one road and one settlement
            sentBy.GameData.Resources.GrantEntitlement(Entitlement.Road);
            sentBy.GameData.Resources.GrantEntitlement(Entitlement.Settlement);
            ChangePlayer(gameController, -1);
            return Task.CompletedTask;
        }

        public static Task UndoAllocateForwardResources(IGameController gameController, string sentByName)
        {
            var sentBy = gameController.NameToPlayer(sentByName);
            sentBy.GameData.Resources.RevokeEntitlement(Entitlement.Road);
            sentBy.GameData.Resources.RevokeEntitlement(Entitlement.Settlement);
            ChangePlayer(gameController, -1);
            return Task.CompletedTask;
        }

        public static Task UndoAllocateReverseResources(IGameController gameController, string sentByName)
        {
            var firstPlayer = gameController.MainPageModel.PlayingPlayers[0];

            firstPlayer.GameData.Resources.RevokeEntitlement(Entitlement.Road);
            firstPlayer.GameData.Resources.RevokeEntitlement(Entitlement.Settlement);
            ChangePlayer(gameController, 1);
            return Task.CompletedTask;
        }

        public Task Do(IGameController gameController)
        {
            return WaitingForStartToAllocateResourcesForward.DoAllocateForwardResources(gameController, this.SentBy);
        }

        public Task Redo(IGameController gameController)
        {
            return WaitingForStartToAllocateResourcesForward.DoAllocateForwardResources(gameController, this.SentBy);
        }

        public Task Undo(IGameController gameController)
        {
            return WaitingForStartToAllocateResourcesForward.UndoAllocateForwardResources(gameController, this.SentBy);
        }
    }
}