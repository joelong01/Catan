﻿using System.Threading.Tasks;

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
                SentBy = gameController.CurrentPlayer
            };
            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            var player = gameController.NameToPlayer(this.SentBy);
            player.GameData.Resources.GrantResources(this.TradeResources);
             await Task.Delay(0);
        }

        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            var player = gameController.NameToPlayer(this.SentBy);
            player.GameData.Resources.GrantResources(this.TradeResources.GetNegated());
             await Task.Delay(0);
        }
    }
}