using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    public class CreateGameModel : LogHeader, ILogController
    {
        public GameInfo GameInfo { get; set; }
        public static CatanMessage CreateMessage (GameInfo gameInfo, string playerName)
        {
            var model = new CreateGameModel
            {
                Action = CatanAction.GameCreated,
                CanUndo = false,
                CatanGame = CatanGames.Regular,
                LogId = default,
                LogType = LogType.Normal,
                NewState = GameState.WaitingForPlayers,
                OldState = GameState.Uninitialized,
                Previous = null,
                SentBy = MainPage.Current.NameToPlayer(playerName),
                CreatedTime = DateTime.Now,                
                GameInfo = gameInfo
            };

            var message = new CatanMessage
            {
                
                ActionType = ActionType.Normal,
                Data = (object)model,
                DataTypeName = typeof(CreateGameModel).FullName,
                From = playerName,
                MessageId = Guid.NewGuid(),
                MessageType = MessageType.CreateGame,
                Sequence = 0,
                To = "",                
            };
            return message;
        }
        public static Task CreateGame(IGameController gameController, GameInfo gameInfo)
        {
            var logHeader = new CreateGameModel()
            {
                GameInfo = gameInfo,
                NewState = GameState.WaitingForPlayers

            };

            return gameController.ExecuteSynchronously(logHeader, ActionType.Normal, MessageType.CreateGame);
        }

        public Task Do(IGameController gameController)
        {
            return gameController.Proxy.CreateGame(this.GameInfo);
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public async Task Replay (IGameController gameController)
        {
            var games = await gameController.Proxy.GetAllGames();
            foreach (var game in games)
            {
                if (game.Id == this.GameInfo.Id)
                {
                    return;
                }

                if (game.Name == this.GameInfo.Name)
                {
                    Debug.Assert(false, "Ids should be the same");
                    return;
                }
            }

            //
            //  got here -- game has died.  create it.
            await Do(gameController);
        }
    }
}
