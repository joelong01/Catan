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
                SentBy = player.PlayerName,
                Action = CatanAction.Purchased,
                PurchasedEntitlement = entitlement
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            PlayerModel player = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(player != null);
            var cost = TradeResources.GetEntitlementCost(PurchasedEntitlement);
            player.GameData.Resources.Current += cost.GetNegated();
            if (PurchasedEntitlement == Entitlement.DevCard)
            {
                DevCardType devCard = gameController.PurchaseNextDevCard();
                if (devCard == DevCardType.None) // ran out of DevCards!
                {
                    player.GameData.Resources.Current += cost; //refund!
                    return Task.CompletedTask;
                }

                player.GameData.Resources.AddDevCard(devCard);
            }
            else
            {
                player.GameData.Resources.GrantEntitlement(PurchasedEntitlement);
            }
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            PlayerModel player = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(player != null);
            Contract.Assert(player.GameData.Resources.UnspentEntitlements.Contains(PurchasedEntitlement));
            Contract.Assert(PurchasedEntitlement != Entitlement.DevCard);
            var cost = TradeResources.GetEntitlementCost(PurchasedEntitlement);
            player.GameData.Resources.Current += cost;
            player.GameData.Resources.RevokeEntitlement(PurchasedEntitlement);

            return Task.CompletedTask;
        }
    }
}
