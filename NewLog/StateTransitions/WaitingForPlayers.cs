
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class WaitingForNewGameToWaitingForPlayers : LogHeader, ILogController
    {
        public WaitingForNewGameToWaitingForPlayers() : base()
        {
        }

        public static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForNewGame);

            WaitingForNewGameToWaitingForPlayers logHeader = new WaitingForNewGameToWaitingForPlayers()
            {
                CanUndo = false,
                Action = CatanAction.ChangedState,
                OldState = GameState.WaitingForNewGame,
                NewState = GameState.WaitingForPlayers,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        /// <summary>
        ///     Transitions to the WaitingForPlayers state
        ///     Monitor will add the logs
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public async Task Do(IGameController gameController)
        {
            MainPageModel mpm = gameController.MainPageModel;
            if (mpm.Settings.AutoRespond && mpm.GameStartedBy == gameController.TheHuman)
            {
                await AddPlayerLog.AddPlayer(gameController, gameController.TheHuman);
            }
        }

        /// <summary>
        ///     Nothing to do here except add the log, which the monitor will do
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public Task Redo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"[Action={Action}][SentBy={SentBy}][OldState={OldState}][NewState={NewState}]";
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }
    }
}