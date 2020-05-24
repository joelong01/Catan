using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     This class has all the data associated with adding a player to the game
    /// </summary>
    public class AddPlayerLog : LogHeader, ILogController
    {
        public string PlayerToAdd { get; set; }
        public AddPlayerLog() : base()
        {
            Action = CatanAction.AddPlayer;
        }

        public static async Task AddPlayer(IGameController gameController, PlayerModel playerToAdd)
        {
            AddPlayerLog logHeader = new AddPlayerLog
            {
                PlayerToAdd = playerToAdd.PlayerName,
                SentBy = gameController.TheHuman.PlayerName,
                NewState = GameState.WaitingForPlayers,
                OldState = gameController.CurrentGameState,
                CanUndo = false
            };
            
            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return gameController.AddPlayer(this);
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.AddPlayer(this);
        }

        public override string ToString()
        {
            return $"[AddedPlayer={PlayerToAdd}"  + base.ToString();
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.UndoAddPlayer(this);
        }
    }
}