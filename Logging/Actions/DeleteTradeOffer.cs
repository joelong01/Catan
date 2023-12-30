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
        public async Task Do(IGameController gameController)
        {
            TradeOffer localOffer = gameController.TheHuman.GameData.Trades.FindTradeByValue(this.TradeOffer);
            if (localOffer == null)
            {
                //
                //   this might happen if the user has deleted an offer locally (e.g. not the owner) and then 
                //   the owner subsequently deletes it.  not a problem.                
                await Task.Delay(0);
            }

            gameController.TheHuman.GameData.Trades.PotentialTrades.Remove(localOffer);
            await Task.Delay(0);
        }

        public async Task Redo(IGameController gameController)
        {
             await Task.Delay(0);
        }

        public Task Replay (IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public async Task Undo(IGameController gameController)
        {
            await Do(gameController);
        }
    }
}
