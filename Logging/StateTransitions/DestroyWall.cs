﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10.Logging.StateTransitions
{
    internal class DestroyWall : LogHeader, ILogController
    {
        int Index { get; set; }
        public static async Task PostDestroyWall(IGameController gameController, BuildingCtrl building)
        {
            var logEntry = new DestroyWall()
            {
                Index = building.Index
            };

            await gameController.PostMessage(logEntry, ActionType.Normal);

        }
        public async Task Do(IGameController gameController)
        {
            BuildingCtrl building = gameController.GetBuilding(this.Index);
            building.City.HasWall = false;
            gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.DestroyCity);
             await Task.Delay(0);
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
            BuildingCtrl building = gameController.GetBuilding(this.Index);
            building.City.HasWall = true;
             await Task.Delay(0);
        }
    }
}