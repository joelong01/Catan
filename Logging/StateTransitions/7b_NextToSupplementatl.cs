
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    public class WaitingForNextToPickSupplementalPlayers : LogHeader, ILogController
    {
        public Guid StartPlayerId { get; set; }
        public static async Task PostLog(IGameController gameController)
        {
            var logHeader = new WaitingForNextToPickSupplementalPlayers()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.PickSupplementalPlayers,
                StartPlayerId = gameController.CurrentPlayer.PlayerIdentifier

            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            await gameController.ResetRollControl();
            gameController.StopHighlightingTiles();
            gameController.MainPageModel.SupplementalPlayers.Clear();
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
            await DefaultTask;
        }
    }

    /// <summary>
    ///     Transition into the Supplemental build phase
    /// </summary>
    public class PickingPlayersToSupplemental : LogHeader, ILogController
    {
        public DevCardModel DevCardPlayedThisTurn { get; private set; }
        public Guid NextPlayerId { get; set; }
        public Guid CurrentPlayerId { get; set; }

        public static async Task PostLog(IGameController gameController)
        {


            PickingPlayersToSupplemental logHeader = new PickingPlayersToSupplemental()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.Supplemental,
                DevCardPlayedThisTurn = gameController.CurrentPlayer.GameData.Resources.ThisTurnsDevCard,

                CurrentPlayerId = gameController.CurrentPlayer.PlayerIdentifier,
                NextPlayerId = gameController.MainPageModel.SupplementalPlayers[0].PlayerIdentifier
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }
        /// <summary>
        ///     this needs to move to the first player that is doing supplemental
        ///     t
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public async Task Do(IGameController gameController)
        {


            ChangePlayerHelper.ChangePlayerTo(gameController, NextPlayerId);
            gameController.MainPageModel.SupplementalPlayers.RemoveAt(0);
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
            gameController.CurrentPlayer.GameData.Resources.ThisTurnsDevCard = this.DevCardPlayedThisTurn;
            await gameController.ResetRollControl();
            gameController.StopHighlightingTiles();
            ChangePlayerHelper.ChangePlayerTo(gameController, CurrentPlayerId);
            var player = gameController.PlayerFromId(NextPlayerId);
            gameController.MainPageModel.SupplementalPlayers.Insert(0, player);

        }
    }
}

