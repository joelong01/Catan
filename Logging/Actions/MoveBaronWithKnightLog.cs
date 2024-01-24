using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    internal class MoveBaronWithKnightLog : LogHeader, ILogController
    {
        int KnightIndex { get; set; }
       
        public static async Task PostLog(IGameController gameController, BuildingCtrl building)
        {
            if (gameController.CurrentGameState != GameState.ClickOnKnight && gameController.CurrentGameState != GameState.WaitingForRoll)
            {
                Debug.Assert(false, $"strange -- investigate.  currentstate is {gameController.CurrentGameState}");
            }

            MoveBaronWithKnightLog logHeader = new MoveBaronWithKnightLog()
            {
                NewState = GameState.MustMoveBaron,
                CanUndo = true,
                KnightIndex = building.Index,
                UndoNext = false,

            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.MoveBaronWithKnight);
            await MustMoveBaronLog.PostLog(gameController, MoveBaronReason.Knight, true);
            gameController.GetBuilding(KnightIndex).Knight.Activated = false;

            await DefaultTask;
        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Replay(IGameController gameController)
        {

            await DefaultTask;
        }

        public async Task Undo(IGameController gameController)
        {
            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.MoveBaronWithKnight);
            gameController.GetBuilding(KnightIndex).Knight.Activated = true;
            await DefaultTask;
        }
    }
}
