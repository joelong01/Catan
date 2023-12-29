using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;

namespace Catan10
{
    /// <summary>
    ///
    ///
    /// </summary>
    public class ChangeStateToMustDestroyCity : LogHeader, ILogController
    {


        public static async Task PostLog(IGameController gameController)
        {

            ChangeStateToMustDestroyCity logHeader = new ChangeStateToMustDestroyCity()
            {
                NewState = GameState.MustDestroyCity,
                CanUndo = true,


            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            // move current player to the first player that needs to destroy a city
           
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Replay(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }


    }
}