using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    internal class ImprovementLog : LogHeader, ILogController

    {
        private Entitlement Entitlement { get; set; }
        private int CurrentRank { get; set; }

        public static async Task PostLog(IGameController gameController, Entitlement entitlement, int currentRank)
        {
            Debug.Assert(entitlement == Entitlement.TradeUpgrade || entitlement == Entitlement.PoliticsUpgrade || entitlement == Entitlement.ScienceUpgrade);

            var logEntry = new ImprovementLog()
            {
                CurrentRank = currentRank,
                Entitlement = entitlement,
            };

            await gameController.PostMessage(logEntry, ActionType.Normal);

        }

        /// <summary>
        ///     the rules are
        ///     1. first to 5 gets a metro (they have to pick a city to upgrade)
        ///     2. if somebody else goes to 6, the first to 5 palyer looses he metro and the first to 6 picks a city
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public async Task Do(IGameController gameController)
        {


            (int maxRank, PlayerModel maxPlayer) = HighestImprovement(gameController, this.Entitlement);
            (int rank, int metroCityId) = gameController.CurrentPlayer.GameData.GetImprovementRank(this.Entitlement);
            gameController.CurrentPlayer.GameData.SetImprovementRank(this.Entitlement, rank + 1, metroCityId);
            if (rank == 4 && maxRank == 4 || ( rank == 5 && maxRank == 5 && maxPlayer != gameController.CurrentPlayer ))
            {
                Debug.Assert(metroCityId == -1);
                await MetroTransitionLog.PickCityLog(gameController, this.Entitlement);
            }




        }

        (int, PlayerModel) HighestImprovement(IGameController gameController, Entitlement improvement)
        {
            int max = 0;
            PlayerModel maxPlayer = null;
            foreach (var player in gameController.PlayingPlayers)
            {

                (int r, int id) = player.GameData.GetImprovementRank(this.Entitlement);
                {
                    if (r > max)
                    {
                        max = r;
                        maxPlayer = player;
                    }
                }
            }

            if (max == 5) // make sure that is more than 1 person has rank=5, we pick the one that got there first, which will have a CityId set
            {
                foreach (var player in gameController.PlayingPlayers)
                {
                    if (player.GameData.GetImprovementRank(this.Entitlement).BuildingId != -1)
                    {
                        maxPlayer = player;
                        break;
                    }
                }
            }

            return (max, maxPlayer);
        }


        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Replay(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            (int rank, int metroCityId) = gameController.CurrentPlayer.GameData.GetImprovementRank(this.Entitlement);
            gameController.CurrentPlayer.GameData.SetImprovementRank(this.Entitlement, rank - 1, metroCityId);

            await Task.Delay(1);
        }
    }
}
