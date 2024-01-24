using System.Diagnostics;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class ResourceExchangeLog : LogHeader, ILogController
    {
        #region Properties

        public TradeResources Traded { get; set; }

        #endregion Properties

        #region Methods

        public static Task PostLog(IGameController gameController, TradeResources tr)
        {
            Debug.Assert(gameController.CurrentGameState == GameState.WaitingForNext);
            var logHeader = new ResourceExchangeLog()
            {
                Traded = tr,
                Action = CatanAction.TradeResources,
                CanUndo = true
            };

            return gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            //
            //  if a trade happens, I flip the cards showing what resources came from a roll so that we can see what the trade was
            gameController.PlayingPlayers.ForEach((p) => p.GameData.Resources.ResourcesThisTurn.Reset());

            var owner = gameController.NameToPlayer(this.SentBy);
            owner.GameData.Resources.GrantResources(this.Traded);
            await DefaultTask;
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
            var owner = gameController.NameToPlayer(this.SentBy);
            owner.GameData.Resources.GrantResources(this.Traded.GetNegated());
             await DefaultTask;
        }

        #endregion Methods
    }
}