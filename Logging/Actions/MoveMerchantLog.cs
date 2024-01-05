using System;
using Windows.Foundation;

using System.Threading.Tasks;

namespace Catan10
{
    internal class MoveMerchantLog : LogHeader, ILogController
    {
        public Point From { get; set; }
        public Point To { get; set; }   

        public static async Task PostLogMessage(IGameController controller, Point from, Point to)
        {
            MoveMerchantLog logEntry = new MoveMerchantLog()
            {
                From = from,
                NewState = GameState.WaitingForNext
            };

            await controller.PostMessage(logEntry, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.Merchant);
            await Task.Delay(10);
        }

        public async Task Redo(IGameController gameController)
        {
            gameController.MoveMerchant(To);
            await Task.Delay(10);
        }

        public async Task Replay(IGameController gameController)
        {
            gameController.MoveMerchant(To);
            await Task.Delay(10);
        }

        public async Task Undo(IGameController gameController)
        {
            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Merchant);
            gameController.MoveMerchant(From);
            await Task.Delay(10);
        }
    }
}
