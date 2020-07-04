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
        #region Properties + Fields

        public string PlayerToAdd { get; set; }

        #endregion Properties + Fields



        #region Constructors

        public AddPlayerLog() : base()
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     an Action to add a player -- DOES NOT CHANGE STATE
        /// </summary>
        /// <param name="gameController"></param>
        /// <param name="playerToAdd"></param>
        /// <returns></returns>
        public static async Task AddPlayer(IGameController gameController, string playerToAdd)
        {
            //  5/20/2020:  The state *after* the CreateGame logEntry is pused is WaitingForPlayers.  But
            //              NewGameLog.Do() calls AddPlayerLog.PostLog *before* it gets pushed to the stack.
            //              so we have to hard code the new state because by the time we get to the log, the
            //              state will change

            // 6/15: pull the state from the interface as the debugger won't tell you what it is if the contract fails.
            
            GameState state = gameController.CurrentGameState;

            //
            //  6/16/2020:  if the state !=  GameState.WaitingForPlayers then that means that the JoinOrCreateGame call hasn't completed.
            //              which probably means there is an issue calling the service
            //  
            Contract.Assert(state == GameState.WaitingForPlayers); // you can only add players in this state
            AddPlayerLog logHeader = new AddPlayerLog
            {
                Action = CatanAction.AddPlayer,
                PlayerToAdd = playerToAdd,
                SentBy = gameController.TheHuman,
                NewState = GameState.WaitingForPlayers,
                OldState = gameController.CurrentGameState,
                CanUndo = false
            };

            await gameController.ExecuteSynchronously(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            await gameController.AddPlayer(this.PlayerToAdd);
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.AddPlayer(this.PlayerToAdd);
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