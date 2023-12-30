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

        public static async Task CreateGame(IGameController gameController, GameInfo gameInfo, CatanAction action)
        {
            Contract.Assert(action == CatanAction.GameCreated || action == CatanAction.GameJoined);

            NewGameLog logHeader = new NewGameLog
            {
                GameInfo = gameInfo,
                SentBy = gameController.TheHuman,
                NewState = GameState.WaitingForPlayers,
                OldState = gameController.Log.PeekAction?.NewState ?? GameState.Uninitialized,
                Action = action,                
                CanUndo = false
            };

            await gameController.ExecuteSynchronously(logHeader, ActionType.Normal, action == CatanAction.GameCreated ? MessageType.CreateGame : MessageType.JoinGame);
        }

        public async Task Do(IGameController gameController)
        {
            await gameController.CreateGame(this.GameInfo);

        }

        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Redo(IGameController gameController)
        {
            return this.Do(gameController);
        }

        public override string ToString()
        {
            return $"StartGame: [StartedBy={GameInfo.Creator}][SendBy={SentBy}[id={LogId}]";
        }

        public async Task Undo(IGameController gameController)
        {
            await Task.Delay(0);
        }

        #endregion Methods
    }
}