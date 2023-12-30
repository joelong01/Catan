using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     this knows how to buy
    /// </summary>
    public class PurchaseLog : LogHeader, ILogController
    {
        public Entitlement PurchasedEntitlement { get; set; }

        public static async Task PostLog(IGameController gameController, PlayerModel player, Entitlement entitlement)
        {
            PurchaseLog logHeader = new PurchaseLog()
            {
                CanUndo = entitlement != Entitlement.DevCard,
                SentBy = player,
                Action = CatanAction.Purchased,
                PurchasedEntitlement = entitlement
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            PlayerModel player = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(player != null);
            var cost = TradeResources.GetEntitlementCost(PurchasedEntitlement);
            player.GameData.Resources.CurrentResources += cost.GetNegated();
            if (PurchasedEntitlement == Entitlement.DevCard)
            {
                DevCardType devCard = gameController.PurchaseNextDevCard();
                if (devCard == DevCardType.None) // ran out of DevCards!
                {
                    player.GameData.Resources.CurrentResources += cost; //refund!
                    await Task.Delay(0);
                }

                player.GameData.Resources.AddDevCard(devCard);
            }
            else
            {
                player.GameData.Resources.GrantEntitlement(PurchasedEntitlement);
            }
            await Task.Delay(0);
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
            PlayerModel player = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(player != null);
            Contract.Assert(player.GameData.Resources.UnspentEntitlements.Contains(PurchasedEntitlement));
            Contract.Assert(PurchasedEntitlement != Entitlement.DevCard);
            var cost = TradeResources.GetEntitlementCost(PurchasedEntitlement);
            player.GameData.Resources.CurrentResources += cost;
            player.GameData.Resources.RevokeEntitlement(PurchasedEntitlement);

            await Task.Delay(0);
        }
    }
}