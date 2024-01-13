﻿
using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Transition into the Supplemental build phase
    /// </summary>
    public class WaitingForNextToSupplemental : LogHeader, ILogController
    {
        public DevCardModel DevCardPlayedThisTurn { get; private set; }

        public static async Task PostLog(IGameController gameController)
        {

            WaitingForNextToSupplemental logHeader = new WaitingForNextToSupplemental()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.Supplemental,
                DevCardPlayedThisTurn = gameController.CurrentPlayer.GameData.Resources.ThisTurnsDevCard
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            await gameController.ResetRollControl();
            gameController.StopHighlightingTiles();
            ChangePlayerHelper.ChangePlayer(gameController, 1);

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
            gameController.CurrentPlayer.GameData.Resources.ThisTurnsDevCard = this.DevCardPlayedThisTurn;
            await gameController.ResetRollControl();
            gameController.StopHighlightingTiles();
            ChangePlayerHelper.ChangePlayer(gameController, -1);
            
        }
    }
}

