using Catan.Proxy;
using System.Collections.Generic;
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
        
        public bool ServiceGame { get; set; } = true;
        public static async Task<StartGameLog> StartGame(IGameController gameController, string startingPlayer, int gameIndex, bool serviceGame)
        {
            StartGameLog model = new StartGameLog
            {
                
                PlayerName = startingPlayer,
                ServiceGame = serviceGame,
                NewState= GameState.WaitingForStart,
                OldState= GameState.WaitingForNewGame,
                Action = CatanAction.GameCreated,
                GameIndex = gameIndex,

            };
            await gameController.StartGame(model);
            return model;

        }

        public Task Do(IGameController gameController, LogHeader logHeader)
        {
            throw new System.NotImplementedException();
        }

        public Task Redo(IGameController gameController, LogHeader logHeader)
        {

            throw new System.NotImplementedException();
        }

        public Task Undo(IGameController gameController, LogHeader logHeader)
        {
            throw new System.NotImplementedException();
        }
    }

}