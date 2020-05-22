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
            return $"[CreatedBy={CreatedBy}][AddPlayer={PlayerName}] [Local={LocallyCreated}]";
        }

        public static async Task AddPlayer(IGameController gameController, PlayerModel playerModel)
        {

            AddPlayerLog logHeader = new AddPlayerLog
            {
                PlayerName = playerModel.PlayerName,
                CanUndo = false
            };
            bool serviceGame = await gameController.PostMessage(logHeader, CatanMessageType.Normal);
            
            
            
        }

        public Task Do(IGameController gameController)
        {
            return gameController.AddPlayer(this);
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.AddPlayer(this);
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.UndoAddPlayer(this);
        }

    }
}
