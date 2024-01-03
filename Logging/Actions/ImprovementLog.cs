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
        public async Task Do(IGameController gameController)
        {
            switch (Entitlement)
            {

                case Entitlement.PoliticsUpgrade:
                    gameController.CurrentPlayer.GameData.PoliticsRank++;
                    break;
                case Entitlement.ScienceUpgrade:
                    gameController.CurrentPlayer.GameData.ScienceRank++;
                    break;
                case Entitlement.TradeUpgrade:
                    gameController.CurrentPlayer.GameData.TradeRank++;
                    break;

                default:
                    Debug.Assert(false, "Entitlement should be one of the above!");
                    break;
            }

            await Task.Delay(1);

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
            switch (Entitlement)
            {

                case Entitlement.PoliticsUpgrade:
                    gameController.CurrentPlayer.GameData.PoliticsRank--;
                    Debug.Assert(CurrentRank == gameController.CurrentPlayer.GameData.PoliticsRank);
                    break;
                case Entitlement.ScienceUpgrade:
                    gameController.CurrentPlayer.GameData.ScienceRank--;
                    Debug.Assert(CurrentRank == gameController.CurrentPlayer.GameData.ScienceRank);
                    break;
                case Entitlement.TradeUpgrade:
                    gameController.CurrentPlayer.GameData.TradeRank--;
                    Debug.Assert(CurrentRank == gameController.CurrentPlayer.GameData.TradeRank);
                    break;

                default:
                    Debug.Assert(false, "Entitlement should be one of the above!");
                    break;
            }

            await Task.Delay(1);
        }
    }
}
