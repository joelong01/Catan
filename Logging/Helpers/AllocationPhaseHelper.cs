namespace Catan10
{
    public static class AllocationPhaseHelper
    {
        #region Methods

        public static void GrantEntitlements(IGameController gameController, string to)
        {
            var player = gameController.NameToPlayer(to);
            player.GameData.Resources.GrantEntitlement(Entitlement.Road);
            player.GameData.Resources.GrantEntitlement(Entitlement.Settlement);
            if (gameController.CurrentGameState == GameState.AllocateResourceReverse && gameController.MainPageModel.GameInfo.Pirates)
            {
                player.GameData.Resources.GrantEntitlement(Entitlement.City);
            }
        }

        public static void RevokeEntitlements(IGameController gameController, string to)
        {
            var player = gameController.NameToPlayer(to);
            player.GameData.Resources.RevokeEntitlement(Entitlement.Road);
            player.GameData.Resources.RevokeEntitlement(Entitlement.Settlement);
            if (gameController.CurrentGameState == GameState.AllocateResourceReverse && gameController.MainPageModel.GameInfo.Pirates)
            {
                player.GameData.Resources.RevokeEntitlement(Entitlement.City);
            }
        }

        #endregion Methods
    }
}