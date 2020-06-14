using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class TradeGold : LogHeader, ILogController
    {
        #region Properties

        public TradeResources GoldTrade { get; set; }

        #endregion Properties

        #region Methods

        public static async Task PostTradeMessage(IGameController gameController, TradeResources tradeResources)
        {
            Contract.Assert(tradeResources.Count == 0);
            TradeGold logHeader = new TradeGold()
            {
                GoldTrade = tradeResources
            };
            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            var player = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(player != null);

            player.GameData.Resources.GrantResources(this.GoldTrade);
            //
            //  if anybody has a gold card, do nothing else
            foreach (var p in gameController.PlayingPlayers)
            {
                if (p.GameData.Resources.Current.GetCount(ResourceType.GoldMine) > 0)
                {
                    return Task.CompletedTask;
                }
            }

            if (gameController.CurrentPlayer == gameController.TheHuman)
            {
                //
                //  otherwise set the state so things can continue along
                return TradeGoldToWaitingForNext.PostMessage(gameController);
            }

            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}