using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     this sets the state to M
    /// </summary>

    public class MustMoveBaronToWaitingForNext : LogHeader, ILogController
    {
        public BaronModel BaronModel { get; set; }

        public static async Task PostLog(IGameController gameController, PlayerModel victim, int targetTileIndex, int previousIndex, TargetWeapon weapon)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.MustMoveBaron);

            MustMoveBaronToWaitingForNext logHeader = new MustMoveBaronToWaitingForNext()
            {
                CanUndo = true,
                Action = CatanAction.AssignedBaron,
                NewState = GameState.WaitingForNext,
                BaronModel = new BaronModel()
                {
                    Victim = (victim == null) ? "" : victim.PlayerName,
                    Weapon = weapon,
                    PreviousTile = previousIndex,
                    TargetTile = targetTileIndex
                }
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            PlayerModel targetPlayer = null;
            if (this.BaronModel.Victim != null && this.BaronModel.Victim != "")
                targetPlayer = gameController.NameToPlayer(this.BaronModel.Victim);

            var targetTile = gameController.TileFromIndex(this.BaronModel.TargetTile);
            var previousTile = gameController.TileFromIndex(this.BaronModel.PreviousTile);
            var weapon = this.BaronModel.Weapon;

            if (targetPlayer != null)
            {
                targetPlayer.GameData.TimesTargeted++;
            }

            if (weapon == TargetWeapon.PirateShip)
            {
                gameController.GameContainer.PirateShipTile = targetTile;
            }
            else
            {
                gameController.GameContainer.BaronTile = targetTile;
            }

            return Task.CompletedTask;  // state is now Waiting for next
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            PlayerModel targetPlayer = gameController.NameToPlayer(this.BaronModel.Victim);
            var previousTile = gameController.TileFromIndex(this.BaronModel.PreviousTile);
            var weapon = this.BaronModel.Weapon;

            targetPlayer.GameData.TimesTargeted--;

            if (weapon == TargetWeapon.PirateShip)
            {
                gameController.GameContainer.PirateShipTile = previousTile;
            }
            else
            {
                gameController.GameContainer.BaronTile = previousTile;
            }

            gameController.CurrentPlayer.GameData.MovedBaronAfterRollingSeven = false;

            return Task.CompletedTask;
        }
    }
}
