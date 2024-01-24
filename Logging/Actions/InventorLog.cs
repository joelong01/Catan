using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    internal class InventorLog : LogHeader, ILogController
    {
        int TargetTileIndex { get;set; }
        int SourceTargetTileIndex { get;set; }

        public static async Task PostLogMessage(IGameController gameController, TileCtrl source, TileCtrl target)
        {
            var log = new InventorLog()
            {
                NewState = GameState.WaitingForNext,
                TargetTileIndex = target.Index,
                SourceTargetTileIndex = source.Index,
            };
            await gameController.PostMessage(log, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            TileCtrl source = gameController.TileFromIndex(SourceTargetTileIndex);
            TileCtrl target = gameController.TileFromIndex(TargetTileIndex);
            (target.Number, source.Number) = (source.Number, target.Number);
            source.CatanNumber.MoveAsync(new Windows.Foundation.Point(0,0));
            gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.Inventor);
            await DefaultTask;
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

            TileCtrl source = gameController.TileFromIndex(SourceTargetTileIndex);
            TileCtrl target = gameController.TileFromIndex(TargetTileIndex);
            (source.Number, target.Number) = (target.Number, source.Number) ;
            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Inventor);
            await DefaultTask;
        }
    }
}
