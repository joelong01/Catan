using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public static class AllocationPhaseHelper
    {
        public static void GrantEntitlements(IGameController gameController, string to)
        {
            var player = gameController.NameToPlayer(to);
            player.GameData.Resources.GrantEntitlement(Entitlement.Road);
            player.GameData.Resources.GrantEntitlement(Entitlement.Settlement);
        }

        public static void RevokeEntitlements(IGameController gameController, string to)
        {
            var player = gameController.NameToPlayer(to);
            player.GameData.Resources.RevokeEntitlement(Entitlement.Road);
            player.GameData.Resources.RevokeEntitlement(Entitlement.Settlement);
        }
    }
}
