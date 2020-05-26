﻿
using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class PickingBoardToWaitingForRollOrder : LogHeader, ILogController
    {
        public static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.PickingBoard);

            PickingBoardToWaitingForRollOrder logHeader = new PickingBoardToWaitingForRollOrder()
            {
                CanUndo = false,
                Action = CatanAction.ChangedState,
                OldState = GameState.PickingBoard,
                NewState = GameState.WaitingForRollForOrder,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            //
            //  turn off pips

            gameController.ResetAllBuildings();
            MainPageModel mainPageModel = gameController.MainPageModel;

            if (mainPageModel.Settings.AutoRespond)
            {
                Random rand = new Random();
                await SynchronizedRollLog.StartSyncronizedRoll(gameController, rand.Next(1, 7), rand.Next(1, 7));
            }

           
        }
        /// <summary>
        ///     Redo the action from the log
        ///     Don't call Do as we don't want to do the AutoRespond actions
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public Task Redo(IGameController gameController)
        {

            gameController.ResetAllBuildings();
            MainPageModel mainPageModel = gameController.MainPageModel;
            return Task.CompletedTask; 
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }
    }
}
