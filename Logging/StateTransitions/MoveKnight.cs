using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    public class MoveKnightLog : LogHeader, ILogController
    {
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }
        public KnightRank KnightRank { get; set; }

        public static async Task PostLog(IGameController gameController, BuildingCtrl from, BuildingCtrl to)
        {
            Debug.Assert(from.IsKnight);
            Debug.Assert(!to.IsKnight);
            Debug.Assert(to.BuildingState == BuildingState.None);
            MoveKnightLog logHeader = new MoveKnightLog()
            {
                FromIndex = from.Index,
                ToIndex = to.Index,
                KnightRank = from.Knight.KnightRank
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {

            BuildingCtrl fromBuilding = gameController.GetBuilding(this.FromIndex);
            BuildingCtrl toBuilding = gameController.GetBuilding(this.ToIndex);
            Contract.Assert(fromBuilding != null);
            Contract.Assert(toBuilding != null);
            PlayerModel player = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(player != null);



            await fromBuilding.UpdateBuildingState(player, BuildingState.Knight, BuildingState.None);
            await toBuilding.UpdateBuildingState(player, BuildingState.None, BuildingState.Knight);
            toBuilding.Knight.KnightRank = this.KnightRank;
            fromBuilding.Knight.KnightRank = KnightRank.Basic;
            toBuilding.Knight.Activated = false;
            player.GameData.Resources.ConsumeEntitlement(Entitlement.MoveKnight);


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

            BuildingCtrl to = gameController.GetBuilding(this.ToIndex); 
            BuildingCtrl from = gameController.GetBuilding(this.FromIndex);
            Contract.Assert(to != null);
            Contract.Assert(from != null);
            PlayerModel player = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(player != null);


            await to.UpdateBuildingState(player, BuildingState.Knight, BuildingState.None);
            await from.UpdateBuildingState(player, BuildingState.None, BuildingState.Knight);
            from.Knight.KnightRank = this.KnightRank;
            to.Knight.KnightRank = KnightRank.Basic;
            from.Knight.Activated = true; // all knights that are moved start activated
            player.GameData.Resources.GrantEntitlement(Entitlement.MoveKnight);
        }
    }
}
