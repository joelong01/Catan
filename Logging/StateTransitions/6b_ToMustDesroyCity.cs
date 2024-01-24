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

        public async Task Do(IGameController gameController)
        {
            // move current player to the first player that needs to destroy a city
           
             await DefaultTask;
        }

        public async Task Redo(IGameController gameController)
        {
             await DefaultTask;
        }

        public async Task Replay(IGameController gameController)
        {
             await DefaultTask;
        }

        public async Task Undo(IGameController gameController)
        {
             await DefaultTask;
        }

    }
}