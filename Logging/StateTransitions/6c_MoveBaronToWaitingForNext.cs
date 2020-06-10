using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     UI Prompt to move the Baron or Ship
    /// </summary>
    public class WaitingForNextToMustMoveBaron : LogHeader, ILogController
    {
        public static Task PostLog(IGameController gameController)
        {
            var logHeader = new WaitingForNextToMustMoveBaron()
            {
                CanUndo = true,
                Action = CatanAction.RolledSeven,
                NewState = GameState.MustMoveBaron,
            };

            return gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return Task.CompletedTask; // picks up in PageCallback.TileRightTapped, which calls PageCallback.Baron_MenuClicked
        }

        public Task Redo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }
    }
}
