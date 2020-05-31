using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     This class has all the data associated with adding a player to the game
    /// </summary>
    public class AddPlayerLog : LogHeader, ILogController
    {
        #region Constructors

        public AddPlayerLog() : base()
        {
        }

        #endregion Constructors

        #region Properties

        public string PlayerToAdd { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        ///     an Action to add a player -- DOES NOT CHANGE STATE
        /// </summary>
        /// <param name="gameController"></param>
        /// <param name="playerToAdd"></param>
        /// <returns></returns>
        public static async Task AddPlayer(IGameController gameController, PlayerModel playerToAdd)
        {
            //  5/20/2020:  The state *after* the CreateGame logEntry is pused is WaitingForPlayers.  But
            //              NewGameLog.Do() calls AddPlayerLog.PostLog *before* it gets pushed to the stack.
            //              so we have to hard code the new state because by the time we get to the log, the
            //              state will change

            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForPlayers); // you can only add players in this state
            AddPlayerLog logHeader = new AddPlayerLog
            {
                Action = CatanAction.AddPlayer,
                PlayerToAdd = playerToAdd.PlayerName,
                SentBy = gameController.TheHuman.PlayerName,
                NewState = GameState.WaitingForPlayers,
                OldState = gameController.CurrentGameState,
                CanUndo = false
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            await gameController.AddPlayer(this);

            if (gameController.CurrentPlayer != gameController.MainPageModel.GameStartedBy)
            {
                await ChangePlayerLog.SetCurrentPlayer(gameController, gameController.MainPageModel.GameStartedBy);
            }
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.AddPlayer(this);
        }

        public override string ToString()
        {
            return $"[AddedPlayer={PlayerToAdd}" + base.ToString();
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.UndoAddPlayer(this);
        }

        #endregion Methods
    }
}
