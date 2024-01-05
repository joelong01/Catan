using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    internal class PurchaseMerchantEntitlement : LogHeader, ILogController
    {
        public static async Task PostLogMessage(IGameController controller)
        {
            PurchaseMerchantEntitlement logEntry = new PurchaseMerchantEntitlement()
            {
                NewState = GameState.MustMoveMerchant
            };

            await controller.PostMessage(logEntry, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Merchant);
            await Task.Delay(0);
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
            gameController.CurrentPlayer.GameData.Resources.RevokeEntitlement(Entitlement.Merchant);
            await Task.Delay(0);
        }
    }
}
