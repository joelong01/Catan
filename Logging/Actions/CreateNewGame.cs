using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    public class CreateNewGame : LogHeader, ILogController
    {
        public GameInfo GameInfo { get; set; }
        public static Task CreateGame(IGameController gameController, GameInfo gameInfo)
        {
            var logHeader = new CreateNewGame()
            {
                GameInfo = gameInfo,
                NewState = GameState.WaitingForPlayers

            };

            return gameController.ExecuteSynchronously(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return gameController.Proxy.CreateGame(this.GameInfo);
        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }
    }
}
