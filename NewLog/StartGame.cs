using Catan.Proxy;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Catan10
{

    /// <summary>
    ///     knows how to start a game
    /// </summary>
    public class StartGameLog : LogHeader, ILogController
    {
        public StartGameLog() : base()
        {
            Action = CatanAction.StartGame;
        }
        public int GameIndex { get; set; }
        public override string ToString()
        {
            return $"StartGame: [StartedBy={PlayerName}]";
        }

        
        public static async Task StartGame(IGameController gameController, string startingPlayer, int gameIndex)
        {

            StartGameLog logHeader = new StartGameLog
            {

                PlayerName = startingPlayer,                
                NewState = GameState.WaitingForPlayers,
                OldState = GameState.WaitingForNewGame, // this is a lie -- you can start a new game whenever you want.  
                Action = CatanAction.GameCreated,
                GameIndex = gameIndex,
                CanUndo = false

            };
           
            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
            //
            //  StartGame is a special thing because Monitor() listens based on the Game, so Monitor won't get the message
            //  We post it so the WebSocket layer will send out the message, but since we are sending the message here, we
            //  start the game locally.
            //
            await logHeader.Do(gameController);
            await gameController.Log.PushAction(logHeader);
                        
        }

        public Task Do(IGameController gameController)
        {
            return gameController.StartGame(this);

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