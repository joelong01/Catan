using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///
    ///  actually move the Baron or Ship
    ///
    ///
    ///
    /// </summary>
    public class MustMoveBaronToWaitingForNext : LogHeader, ILogController
    {
        public int PreviousTile { get; set; }
        public int TargetTile { get; set; }
        public string Victim { get; set; }
        public TargetWeapon Weapon { get; set; }

        public static async Task PostLog(IGameController gameController, PlayerModel victom, int targetTileIndex, int previousIndex, TargetWeapon weapon)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRoll);

            MustMoveBaronToWaitingForNext logHeader = new MustMoveBaronToWaitingForNext()
            {
                CanUndo = true,
                Action = CatanAction.AssignedBaron,
                NewState = GameState.WaitingForNext,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            PlayerModel targetPlayer = gameController.NameToPlayer(this.Victim);
            var targetTile = gameController.TileFromIndex(this.TargetTile);
            var previousTile = gameController.TileFromIndex(this.PreviousTile);
            var weapon = this.Weapon;

            targetPlayer.GameData.TimesTargeted++;

            if (weapon == TargetWeapon.PirateShip)
            {
                gameController.GameContainer.PirateShipTile = targetTile;
            }
            else
            {
                gameController.GameContainer.BaronTile = targetTile;
            }

            gameController.CurrentPlayer.GameData.MovedBaronAfterRollingSeven = true;

            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            PlayerModel targetPlayer = gameController.NameToPlayer(this.Victim);
            var previousTile = gameController.TileFromIndex(this.PreviousTile);
            var weapon = this.Weapon;

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

    /// <summary>
    ///     UI Prompt to move the Baron or Ship
    /// </summary>
    public class WaitingForNextToMustMoveBaron : LogHeader, ILogController
    {
        public static Task PostLog(IGameController gameController)
        {
            var logHeader = new WaitingForNextToMustMoveBaron()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.MustMoveBaron,
            };

            return gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }
    }
}
