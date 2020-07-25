using System.Collections.Generic;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     This class has all the data associated with adding a player to the game
    /// </summary>
    public class TestGrantEntitlements : LogHeader, ILogController
    {
        public List<DevCardType> DevCards { get; set; }
        public List<Entitlement> Entitlements { get; set; }
        public TradeResources TradeResources { get; set; }

        public static async Task Post(IGameController gameController, TradeResources tradeResources, List<Entitlement> entitlements, List<DevCardType> devCards)
        {
            TestGrantEntitlements logHeader = new TestGrantEntitlements()
            {
                SentBy = gameController.CurrentPlayer,
                LogType = LogType.DoNotLog,
                Entitlements = entitlements,
                TradeResources = tradeResources,
                DevCards = devCards
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            var player = gameController.NameToPlayer(this.SentBy);
            player.GameData.Resources.GrantResources(this.TradeResources);

            foreach (var entitlement in Entitlements)
            {
                player.GameData.Resources.GrantEntitlement(entitlement);
            }
            foreach (var dc in DevCards)
            {
                var model = new DevCardModel()
                {
                    DevCardType = dc,
                    Played = false,
                };
                player.GameData.Resources.AvailableDevCards.Add(model);
            }

            return Task.CompletedTask;
        }
        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Redo(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }
    }
}