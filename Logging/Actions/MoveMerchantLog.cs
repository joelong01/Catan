using System;
using Windows.Foundation;

using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Catan10
{
    internal class MoveMerchantLog : LogHeader, ILogController
    {
        public Point From { get; set; }
        public Point To { get; set; }
        public Guid PreviousMoverId { get; set; } = Guid.Empty; // you don't need to know this to set it, you need to know to reset it

        public static async Task PostLogMessage(IGameController controller, Point from, Point to)
        {
            Guid playerId = Guid.Empty;
            foreach (var player in controller.PlayingPlayers)
            {
                if (player.GameData.PlayedMerchantLast)
                {
                    playerId = player.PlayerIdentifier;
                    break;
                }
            }

            MoveMerchantLog logEntry = new MoveMerchantLog()
            {
                From = from,
                NewState = GameState.WaitingForNext,
                PreviousMoverId = playerId,
                To = to,
            };

            await controller.PostMessage(logEntry, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            // note setting PlayedMerchantLast updates score, so we need this
            gameController.PlayingPlayers.ForEach((p) => p.GameData.PlayedMerchantLast = false);
            gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.Merchant);
            gameController.CurrentPlayer.GameData.PlayedMerchantLast = true;
            await Task.Delay(0);
        }

        public async Task Redo(IGameController gameController)
        {
            gameController.MoveMerchant(To);
            await Do(gameController);
        }

        public async Task Replay(IGameController gameController)
        {
            gameController.MoveMerchant(To);
            await Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            // note setting PlayedMerchantLast updates score, so we need this
            gameController.PlayingPlayers.ForEach((p) => p.GameData.PlayedMerchantLast = false);
            var previousPlayer =  gameController.PlayerFromId(PreviousMoverId);
            if (previousPlayer != null)
            {
                previousPlayer.GameData.PlayedMerchantLast = true;
            }
            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Merchant);
            gameController.MoveMerchant(From);
            await Task.Delay(0);
        }
    }
}
