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

            return gameController.PostMessage(logHeader, ActionType.Normal);
        }
        public Task Do(IGameController gameController)
        {
            TradeOffer localOffer = gameController.TheHuman.GameData.Trades.FindTradeByValue(this.TradeOffer);
            if (localOffer == null)
            {
               //
               //   this might happen if the user has deleted an offer locally (e.g. not the owner) and then 
               //   the owner subsequently deletes it.  not a problem.                
                return Task.CompletedTask;
            }

            gameController.TheHuman.GameData.Trades.PotentialTrades.Remove(localOffer);
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Replay (IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            return Do(gameController);
        }
    }
}
