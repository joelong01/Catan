using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Catan10
{
    internal class SupplementalToSupplemental : LogHeader, ILogController
    {
        public Guid CurrentPlayerId { get; set; } // so you can move back to it on Undo
        public Guid NextPlayerId { get; set; } // where to go in Do()


        public static async Task PostLog(IGameController gameController)
        {


            SupplementalToSupplemental logHeader = new SupplementalToSupplemental()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.Supplemental,
                CurrentPlayerId = gameController.CurrentPlayer.PlayerIdentifier,
                NextPlayerId = gameController.MainPageModel.SupplementalPlayers[0].PlayerIdentifier

            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            gameController.MainPageModel.SupplementalPlayers.RemoveAt(0);
            ChangePlayerHelper.ChangePlayerTo(gameController, NextPlayerId);
            await DefaultTask;

        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Replay(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            var player = gameController.PlayerFromId(NextPlayerId);
            gameController.MainPageModel.SupplementalPlayers.Insert(0, player);
            ChangePlayerHelper.ChangePlayerTo(gameController, CurrentPlayerId);
            await DefaultTask;
        }
    }
}