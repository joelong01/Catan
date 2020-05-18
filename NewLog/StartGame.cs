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

        public bool ServiceGame { get; set; } = true;
        public static async Task<StartGameLog> StartGame(IGameController gameController, string startingPlayer, int gameIndex, bool serviceGame)
        {

            StartGameLog model = new StartGameLog
            {

                PlayerName = startingPlayer,
                ServiceGame = serviceGame,
                NewState = GameState.WaitingForPlayers,
                OldState = GameState.WaitingForNewGame, // this is a lie -- you can start a new game whenever you want.  
                Action = CatanAction.GameCreated,
                GameIndex = gameIndex,
                CanUndo = false

            };
            await gameController.StartGame(model as StartGameLog);
                     
            return model;

        }

        public Task Do(IGameController gameController, LogHeader logHeader)
        {
            return Task.CompletedTask;
            
        }

        public Task Redo(IGameController gameController, LogHeader logHeader)
        {

            return gameController.StartGame(logHeader as StartGameLog);
        }

        public Task Undo(IGameController gameController, LogHeader logHeader)
        {
            return Task.CompletedTask;
        }
    }

}