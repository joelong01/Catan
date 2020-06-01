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
        #region Constructors

        public WaitingForNewGameToWaitingForPlayers() : base()
        {
        }

        #endregion Constructors

        #region Methods

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
                //
                //  AutoRespond doesn't chane the state because we need to give the other machines a chance to add their own humans
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

        #endregion Methods
    }
}
