using Catan.Proxy;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Catan10
{

    /// <summary>
    ///     knows how to start a game
    /// </summary>
    public class NewGameLog : LogHeader, ILogController
    {
        public NewGameLog() : base()
        {
            Action = CatanAction.StartGame;
        }
        public int GameIndex { get; set; }
        public string CreatedBy { get; set; } = MainPage.Current.TheHuman?.PlayerName;

        public override string ToString()
        {
            return $"StartGame: [StartedBy={CreatedBy}][SendBy={SentBy}[id={LogId}]";
        }

        
        public static async Task NewGame(IGameController gameController, string startingPlayer, int gameIndex)
        {

            NewGameLog logHeader = new NewGameLog
            {
                CreatedBy = startingPlayer,
                SentBy = gameController.TheHuman.PlayerName,                
                NewState = GameState.WaitingForPlayers,
                OldState = (gameController.Log.PeekAction == null) ? GameState.Uninitialized : gameController.Log.PeekAction.NewState,
                Action = CatanAction.GameCreated,
                GameIndex = gameIndex,
                CanUndo = false

            };
           
            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
             
        }

        public async Task Do(IGameController gameController)
        {
            await gameController.StartGame(this);
            await AddPlayerLog.AddPlayer(gameController, gameController.TheHuman);

        }

        public Task Redo(IGameController gameController)
        {

            return gameController.StartGame(this);
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }
    }

}