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

        public string CreatedBy { get; set; } = MainPage.Current.TheHuman?.PlayerName;

        public int GameIndex { get; set; }

        #endregion Properties

        #region Constructors + Destructors

        public NewGameLog() : base()
        {
            Action = CatanAction.StartGame;
        }

        #endregion Constructors + Destructors

        #region Methods

        public static async Task JoinOrCreateGame(IGameController gameController, string startingPlayer, int gameIndex, CatanAction action)
        {
            Contract.Assert(action == CatanAction.GameCreated || action == CatanAction.GameJoined);

            NewGameLog logHeader = new NewGameLog
            {
                CreatedBy = startingPlayer,
                SentBy = gameController.TheHuman.PlayerName,
                NewState = GameState.WaitingForPlayers,
                OldState = (gameController.Log.PeekAction == null) ? GameState.Uninitialized : gameController.Log.PeekAction.NewState,
                Action = action,
                GameIndex = gameIndex,
                CanUndo = false
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            await gameController.JoinOrCreateGame(this);            
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.JoinOrCreateGame(this);
        }

        public override string ToString()
        {
            return $"StartGame: [StartedBy={CreatedBy}][SendBy={SentBy}[id={LogId}]";
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        #endregion Methods
    }
}