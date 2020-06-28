using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    public class TradeApprovalChangedLog : LogHeader, ILogController
    {
        public TradeOffer TradeOffer { get; set; }
        public  static Task ToggleTrade(IGameController gameController, TradeOffer offer)
        {
            TradeApprovalChangedLog logHeader = new TradeApprovalChangedLog()
            {
                TradeOffer = offer,
                CanUndo = false,
                Action = CatanAction.ChangedTradeApproval
            };

            return gameController.PostMessage(logHeader, Catan.Proxy.ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            TradeOffer localOffer = gameController.TheHuman.GameData.Trades.FindTradeByValue(this.TradeOffer);
            if (localOffer == null)
            {
                await StaticHelpers.ShowErrorText("Sorry, your offer no longer exists.\nLooks like somebody beat you to it.", "Catan Trades");
                return;
            }
            localOffer.PartnerApproved = this.TradeOffer.PartnerApproved;
            localOffer.OwnerApproved = this.TradeOffer.OwnerApproved;
            if (localOffer.PartnerApproved && localOffer.OwnerApproved)
            {
                
            }
            
        }

        public Task Redo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }
    }
}
