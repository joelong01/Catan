using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Catan.Proxy;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Graphics.Canvas.Effects;

namespace Catan10
{
    public class KnightStateChangeLog : LogHeader, ILogController
    {
        public int BuildingIndex { get; set; } = -1;
        public KnightRank NewRank { get; set; } = KnightRank.Unset;
        public KnightRank OldRank { get; set; }
        public bool OldActivated { get; set; }
        public bool NewActivated { get; set; }
        public static async Task ToggleActiveState(IGameController gameController, int index, KnightCtrl knight, KnightRank newRank, bool activated)
        {

            var logHeader = new KnightStateChangeLog
            {
                BuildingIndex= index,
                NewRank= newRank,
                OldRank = knight.KnightRank,
                OldActivated= knight.Activated,
                NewActivated= activated,
                UndoNext = false,

            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            var building = gameController.GetBuilding(this.BuildingIndex);
            if (NewActivated && NewActivated != OldActivated) // we activated the knight
            {
                Contract.Assert(gameController.CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.ActivateKnight));
                gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.ActivateKnight);

            }
            building.Knight.Activated = NewActivated;

            if (NewRank != OldRank)
            {
                building.Knight.KnightRank = NewRank;
                gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.BuyOrUpgradeKnight);
            }

            return Task.Delay(0);
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
            var building = gameController.GetBuilding(this.BuildingIndex);
            if (this.OldRank != this.NewRank)
            {
                gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.BuyOrUpgradeKnight);
                building.Knight.KnightRank = this.OldRank;
            }
            if (this.OldActivated != this.NewActivated)
            {
                gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.ActivateKnight);
                building.Knight.Activated = this.OldActivated;
            }

            await Task.Delay(0);
        }
    }
}
