using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    /// <summary>
    ///     This class has all the data associated with adding a player to the game
    /// </summary>
    public class AddPlayerLog : LogHeader, ILogController
    {
        public AddPlayerLog() : base()
        {
            Action = CatanAction.AddPlayer;        
        }
        public override string ToString()
        {
            return $"{PlayerName} [Local={LocallyCreated}]";
        }

        public static async Task<AddPlayerLog> AddPlayer(IGameController gameController, PlayerModel playerModel)
        {

            AddPlayerLog logEntry = new AddPlayerLog
            {
                PlayerName = playerModel.PlayerName,
                CanUndo = false
            };


            await gameController.AddPlayer(logEntry);
            return logEntry; 

        }

        public Task Do(IGameController gameController, LogHeader logHeader)
        {
            return gameController.AddPlayer(logHeader as AddPlayerLog);
        }

        public Task Redo(IGameController gameController, LogHeader logHeader)
        {
            return gameController.AddPlayer(logHeader as AddPlayerLog);
        }

        public Task Undo(IGameController gameController, LogHeader logHeader)
        {
            return gameController.UndoAddPlayer(logHeader as AddPlayerLog);
        }

    }
}
