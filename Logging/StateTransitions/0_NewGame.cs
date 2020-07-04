using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     knows how to start a game
    /// </summary>
    public class NewGameLog : LogHeader, ILogController
    {
        #region Properties

        public GameInfo GameInfo { get; set; }

        #endregion Properties

        #region Constructors + Destructors

        public NewGameLog() : base()
        {
            Action = CatanAction.StartGame;
        }

        #endregion Constructors + Destructors

        #region Methods

        public static async Task JoinOrCreateGame(IGameController gameController, GameInfo gameInfo, CatanAction action)
        {
            Contract.Assert(action == CatanAction.GameCreated || action == CatanAction.GameJoined);

            NewGameLog logHeader = new NewGameLog
            {
                GameInfo = gameInfo,
                SentBy = gameController.TheHuman,
                NewState = GameState.WaitingForPlayers,
                OldState = (gameController.Log.PeekAction == null) ? GameState.Uninitialized : gameController.Log.PeekAction.NewState,
                Action = action,                
                CanUndo = false
            };

            await gameController.ExecuteSynchronously(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            await gameController.JoinOrCreateGame(this.GameInfo);

        }

        public Task Redo(IGameController gameController)
        {
            return this.Do(gameController);
        }

        public override string ToString()
        {
            return $"StartGame: [StartedBy={GameInfo.Creator}][SendBy={SentBy}[id={LogId}]";
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        #endregion Methods
    }
}