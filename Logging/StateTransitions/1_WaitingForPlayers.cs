using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     This is just a UI pause.
    /// </summary>
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
                NewState = GameState.WaitingForPlayers,
            };

            Contract.Assert(logHeader.OldState == GameState.WaitingForNewGame);

            await gameController.PostMessage(logHeader, ActionType.Normal);
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
            if (mpm.Settings.AutoRespond && mpm.GameInfo.Creator == gameController.TheHuman.PlayerName)
            {
                await AddPlayerLog.AddPlayer(gameController, gameController.TheHuman.PlayerName);
                //
                //  AutoRespond doesn't change the state because we need to give the other machines a chance to add their own humans
            }
        }

        public async Task Replay (IGameController gameController)
        {
             await DefaultTask;
        }

        /// <summary>
        ///     Nothing to do here except add the log, which the monitor will do
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public async Task Redo(IGameController gameController)
        {
             await DefaultTask;
        }

        public override string ToString()
        {
            return $"[Action={Action}][SentBy={SentBy}][OldState={OldState}][NewState={NewState}]";
        }

        public async Task Undo(IGameController gameController)
        {
            await DefaultTask;
        }
    }
}