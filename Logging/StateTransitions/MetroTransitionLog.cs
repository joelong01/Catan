using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    enum MetroLogAction { PickCity, UpgradeCity };
    internal class MetroTransitionLog : LogHeader, ILogController
    {
        MetroLogAction MetroLogAction { get; set; }
        int NewMetroId { get; set; }
        int PreviousMetro { get; set; }
        Entitlement Entitlement { get; set; }
        public static async Task PickCityLog(IGameController gameController, Entitlement entitlement)
        {
            var logEntry = new MetroTransitionLog()
            {
                Entitlement = entitlement,
                MetroLogAction = MetroLogAction.PickCity,
                NewState = GameState.UpgradeToMetro,
                UndoNext = true,
            };
            await gameController.PostMessage(logEntry, ActionType.Normal);
        }
        public static async Task UpgradeCityLog(IGameController gameController, int newMetroId)
        {
            //
            // see if anybody already has a metro for this upgrade and if so, note it so we can turn it off
            var prevLogId = gameController.Log.PeekAction as MetroTransitionLog;
            Debug.Assert(prevLogId != null);
            int oldMetroId = -1;
            foreach (var player in gameController.PlayingPlayers)
            {
                (int rank, int cityid) = player.GameData.GetImprovementRank(prevLogId.Entitlement);
                if (cityid != -1)
                {
                    oldMetroId = cityid;
                    break;
                }
            }
            var logEntry = new MetroTransitionLog()
            {
                MetroLogAction = MetroLogAction.UpgradeCity,
                NewState = GameState.WaitingForNext,
                UndoNext = false,
                NewMetroId = newMetroId,
                PreviousMetro = oldMetroId,
                Entitlement = prevLogId.Entitlement
            };
            await gameController.PostMessage(logEntry, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            try
            {
                if (MetroLogAction == MetroLogAction.PickCity)
                {
                    gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.UpgradeToMetro);
                    List<BuildingCtrl> nonMetroCities = new List<BuildingCtrl>();
                    foreach (var b in gameController.CurrentPlayer.GameData.Cities)
                    {
                        if (!b.City.Metropolis)
                        {
                            nonMetroCities.Add(b);
                        }

                    }

                    if (nonMetroCities.Count == 1)
                    {
                        await MetroTransitionLog.UpgradeCityLog(gameController, nonMetroCities[0].Index);

                    }

                    return;
                }

                var building = gameController.GetBuilding(this.NewMetroId);
                if (building.BuildingState != BuildingState.City)
                {
                    Debug.Assert(false, "This should be a city!");
                    return;
                }
                building.City.Metropolis = true;
                (int rank, int buildingId) = gameController.CurrentPlayer.GameData.GetImprovementRank(this.Entitlement);
                Debug.Assert(buildingId == -1); // can't already be a metro
                gameController.CurrentPlayer.GameData.SetImprovementRank(this.Entitlement, rank, building.Index); // same rank, new building id
                gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.UpgradeToMetro);
                if (PreviousMetro != -1)
                {
                    var previous = gameController.GetBuilding(this.PreviousMetro);
                    Debug.Assert(previous.IsCity);
                    previous.City.Metropolis = false;
                }
            }
            finally
            {
                await Task.Delay(0);
            }
        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public Task Replay(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public async Task Undo(IGameController gameController)
        {

            if (MetroLogAction == MetroLogAction.UpgradeCity)
            {
                gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.UpgradeToMetro);



                var building = gameController.GetBuilding(this.NewMetroId);
                Debug.Assert(building.IsCity);
                building.City.Metropolis = false;
                (int rank, int improvedBuildingId) = gameController.CurrentPlayer.GameData.GetImprovementRank(this.Entitlement);
                Debug.Assert(rank > 0);
                Debug.Assert(improvedBuildingId != -1);
                gameController.CurrentPlayer.GameData.SetImprovementRank(this.Entitlement, rank, -1);
                if (PreviousMetro != -1)
                {
                    var previous = gameController.GetBuilding(this.PreviousMetro);
                    Debug.Assert(previous.IsCity);
                    previous.City.Metropolis = true;
                }
            }
            else
            {

                gameController.CurrentPlayer.GameData.Resources.RevokeEntitlement(Entitlement.UpgradeToMetro);
            }
            await Task.Delay(0);
        }
    }
}
