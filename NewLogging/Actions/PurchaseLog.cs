using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catan.Proxy;
using Windows.Media.PlayTo;

namespace Catan10
{
    /// <summary>
    ///     this knows how to buy 
    /// </summary>
    public class PurchaseLog: LogHeader, ILogController
    {
        public Entitlement PurchasedEntitlement { get; set; }
        
        
        public  static async Task PostLog(IGameController gameController, PlayerModel player, Entitlement entitlement)
        {


            PurchaseLog logHeader = new PurchaseLog()
            {
                CanUndo = entitlement != Entitlement.DevCard,
                SentBy = player.PlayerName, 
                Action=CatanAction.Purchased,
                PurchasedEntitlement = entitlement
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
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
                if (devCard == DevCardType.Unknown) // ran out of DevCards!
                {
                    player.GameData.Resources.Current += cost; //refund!
                    return Task.CompletedTask;
                }
            }
            player.GameData.Resources.GrantEntitlement(PurchasedEntitlement);
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
            player.GameData.Resources.GrantEntitlement(PurchasedEntitlement);
            player.GameData.Resources.UnspentEntitlements.Remove(PurchasedEntitlement);
            return Task.CompletedTask;
        }
    }
}
