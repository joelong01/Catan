using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    class DeleteTradeOfferLog : LogHeader, ILogController
    {
        public TradeOffer TradeOffer { get; set; }
        public static Task DeleteOffer(IGameController gameController, TradeOffer offer)
        {
            DeleteTradeOfferLog logHeader = new DeleteTradeOfferLog()
            {
                TradeOffer = offer,
                CanUndo = false,
                Action = CatanAction.DeleteOffer
            };

            return gameController.PostMessage(logHeader, Catan.Proxy.ActionType.Normal);
        }
        public Task Do(IGameController gameController)
        {
            TradeOffer localOffer = gameController.TheHuman.GameData.Trades.FindTradeByValue(this.TradeOffer);
            if (localOffer == null)
            {
                Debug.Assert(false);
                return Task.CompletedTask;
            }

            gameController.TheHuman.GameData.Trades.PotentialTrades.Remove(localOffer);
            return Task.CompletedTask;
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
