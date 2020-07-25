﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     This is a UI pause where
    ///     1. turn off Pips
    ///     2. tell the service the game has started
    /// </summary>
    public class PickingBoardToWaitingForRollOrder : LogHeader, ILogController
    {
        #region Methods

        public static List<RollModel> GetRollModelList()
        {
            List<RollModel> list = new List<RollModel>();
            for (int i = 0; i < 4; i++)
            {
                var model = new RollModel();
                model.Randomize();
                list.Add(model);
            }
            list[0].Selected = true;
            return list;
        }

        public static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.PickingBoard);

            PickingBoardToWaitingForRollOrder logHeader = new PickingBoardToWaitingForRollOrder()
            {
                CanUndo = false,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForRollForOrder,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            //
            //  turn off pips
            gameController.ResetAllBuildings();
            await gameController.TellServiceGameStarted();
            MainPageModel mainPageModel = gameController.MainPageModel;

            if (mainPageModel.Settings.AutoRespond)
            {
                //
                // simulate a roll
                gameController.SimulateRoll(-1);
            }
        }

        public Task Replay (IGameController gameController)
        {
            gameController.ResetAllBuildings();
            return Task.CompletedTask;
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
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        #endregion Methods
    }
}