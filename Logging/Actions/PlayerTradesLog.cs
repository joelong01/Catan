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
        public static Task DoTrade(IGameController gameController, TradeOffer offer)
        {
            PlayerTradesLog logHeader = new PlayerTradesLog()
            {
                TradeOffer = offer,
                CanUndo = false,
                Action = CatanAction.PlayerTrade
            };

            return gameController.PostMessage(logHeader, Catan.Proxy.ActionType.Normal);
        }
        public Task Do(IGameController gameController)
        {

            foreach (var tracker in TradeOffer.TradePartners)
            {
                TradeOffer.Owner = gameController.NameToPlayer(TradeOffer.OwnerName);
                var player = gameController.NameToPlayer(tracker.PlayerName);
                if (player.PlayerIdentifier == TradeOffer.Owner.PlayerIdentifier) continue;
                var o = new TradeOffer()
                {
                    Desire = new TradeResources(TradeOffer.Desire),
                    Offer = new TradeResources(this.TradeOffer.Offer),
                    Owner = TradeOffer.Owner, // set above
                    TradePartners = new ObservableCollection<PlayerTradeTracker>()
                    {
                        new PlayerTradeTracker()
                        {
                            PlayerIdentifier = player.PlayerIdentifier,
                            PlayerName = player.PlayerName,
                            InTrade = true
                        }
                    },
                    OwnerApproved = TradeOffer.OwnerApproved,
                    PartnerApproved = false
                };

                gameController.TheHuman.GameData.Trades.PotentialTrades.Add(o);
            }

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
