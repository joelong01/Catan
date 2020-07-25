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

            return gameController.PostMessage(logHeader, Catan.Proxy.ActionType.Normal);
        }
        public Task Do(IGameController gameController)
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

            return Task.CompletedTask;
        }

        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
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
