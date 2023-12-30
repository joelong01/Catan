using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    class PlayerTradesLog : LogHeader, ILogController
    {
        public TradeOffer TradeOffer { get; set; }
        public List<PlayerModel> TradePartners { get; set; }
        public static Task DoTrade(IGameController gameController, TradeOffer offer, List<PlayerModel> partners)
        {
            PlayerTradesLog logHeader = new PlayerTradesLog()
            {
                TradeOffer = offer,
                TradePartners = partners,
                CanUndo = false,
                Action = CatanAction.PlayerTrade
            };

            return gameController.PostMessage(logHeader, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {

            foreach (PlayerModel player in this.TradePartners)
            {
                
                if (player == TradeOffer.Owner.Player) continue;

                var o = new TradeOffer()
                {
                    Owner = new Offer()
                    {
                        Player = TradeOffer.Owner.Player,
                        Approved = TradeOffer.Owner.Approved,
                        Resources = new TradeResources(TradeOffer.Owner.Resources)
                    },
                    Partner = new Offer()
                    {
                        Player = player,
                        Approved = false,
                        Resources = new TradeResources(TradeOffer.Partner.Resources)
                    }
                };

                gameController.TheHuman.GameData.Trades.PotentialTrades.Add(o);
            }

             await Task.Delay(0);
        }

        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Redo(IGameController gameController)
        {
             await Task.Delay(0);
        }

        public async Task Undo(IGameController gameController)
        {
             await Task.Delay(0);
        }
    }
}
