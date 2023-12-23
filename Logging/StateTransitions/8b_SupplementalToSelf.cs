﻿using Catan.Proxy;
using System.Threading.Tasks;

namespace Catan10
{
    internal class SupplementalToSupplemental : LogHeader, ILogController
    {
        public static async Task PostLog(IGameController gameController)
        {

            SupplementalToSupplemental logHeader = new SupplementalToSupplemental()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.Supplemental,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {

            ChangePlayerHelper.ChangePlayer(gameController, 1);
            return Task.CompletedTask;

        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Replay(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            ChangePlayerHelper.ChangePlayer(gameController, -1);
            return Task.CompletedTask;
        }
    }
}