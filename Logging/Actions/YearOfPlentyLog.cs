using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class YearOfPlentyLog : LogHeader, ILogController
    {
        public TradeResources TradeResources { get; set; }

        public static async Task Post(IGameController gameController, TradeResources tr)
        {
            YearOfPlentyLog logHeader = new YearOfPlentyLog()
            {
                Action = CatanAction.PlayedDevCard,
                TradeResources = tr,
                SentBy = gameController.CurrentPlayer.PlayerName
            };
            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            var player = gameController.NameToPlayer(this.SentBy);
            player.GameData.Resources.GrantResources(this.TradeResources);
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            var player = gameController.NameToPlayer(this.SentBy);
            player.GameData.Resources.GrantResources(this.TradeResources.GetNegated());
            return Task.CompletedTask;
        }
    }
}
