using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Catan.Proxy;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;

namespace Catan10
{
    /// <summary>
    /// after you transition to the "MustMoveBaron" state, this is how you transition back
    /// 
    /// the general flow is 
    /// 
    ///     WaitingForRoll => Rolled 7 => MustMoveBaronLog => MovedBaronLog => WaitingForNext
    ///                             or
    ///     WaitingForRoll => PlayedKnight => MustMoveBaronLog => MovedBaronLog =>  WaitingForRoll
    ///                             or
    ///     WaitingForRoll => WaitingForNext => PlayedKnight => MustMovedBaronLog => MovedBaronLog  => WaitingForRoll
    ///     
    /// </summary>

    public class MovedBaronLog : LogHeader, ILogController
    {
        #region Properties

        public BaronModel BaronModel { get; set; }

        #endregion Properties

        #region Methods
        /// <summary>

        /// <returns></returns>
        public static async Task PostLog(IGameController gameController, List<string> victims, int targetTileIndex, int previousIndex, TargetWeapon weapon, MoveBaronReason reason, ResourceType stolenResource)
        {
            Debug.Assert(gameController.CurrentGameState == GameState.MustMoveBaron);

            // if we are running Knights and Cities, then we can get here via a call to MoveBaronWithKnight and the state we transition to is stored 
            // there.  so here we examine the log to see find the last LogHeader that set the NewState to WaitingForRoll or WaitingForNext and set
            // the new state to that.  it will be the first or second log entry.

            List<GameState> previousStates = new List<GameState>() {GameState.WaitingForNext, GameState.WaitingForRoll };
            var previousLogHeader = gameController.Log.FindLatestLogEntry(previousStates);
            Debug.Assert(previousLogHeader != null);

            bool showBaron = true;
            var previousLargestArmyId = Guid.Empty;
            if (gameController.MainPageModel.GameInfo.CitiesAndKnights)
            {
                if (gameController.InvasionData.TotalInvasions == 0)
                {
                    showBaron = false;
                }
            }
            else
            {
                var playerWithLargestArmy = GetLargestArmyPlayer(gameController);
                if (playerWithLargestArmy != null)
                {
                    var currentLargestArmy = playerWithLargestArmy.GameData.Resources.KnightsPlayed;
                    if (gameController.CurrentPlayer.GameData.Resources.KnightsPlayed == currentLargestArmy) // goint to exceed and get LA!
                    {
                        previousLargestArmyId = playerWithLargestArmy.PlayerIdentifier;
                    }
                }

            }

            // Debug.Assert(((MustMoveBaronLog)gameController.MainPageModel.Log.PeekAction).StartingState == previousState);

            MovedBaronLog logHeader = new MovedBaronLog()
            {
                CanUndo = true,
                Action = CatanAction.MovingBaron,
                NewState = previousLogHeader.NewState,
                BaronModel = new BaronModel()
                {
                    Victims = victims,
                    Weapon = weapon,
                    PreviousTile = previousIndex,
                    TargetTile = targetTileIndex,
                    StolenResource = stolenResource,
                    Reason = reason,
                    MainBaronHidden = showBaron,
                    PreviousLargestArmyPlayerId = previousLargestArmyId,
                }
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public static PlayerModel GetLargestArmyPlayer(IGameController gameController)
        {

            var playerWithLargestArmy = gameController.PlayingPlayers
                    .FirstOrDefault(player => player.GameData.LargestArmy);

            return playerWithLargestArmy;

        }

        public void SetLargestArmy(IGameController controller)
        {
            var playerWithLargestArmy = controller.PlayingPlayers
                    .FirstOrDefault(player => player.GameData.LargestArmy);

            var armySize = controller.CurrentPlayer.GameData.Resources.KnightsPlayed;
            if (armySize < 3) return;

            if (armySize == 3 && playerWithLargestArmy == null)
            {
                controller.CurrentPlayer.GameData.LargestArmy = true;
                return;
            }

            if (armySize > playerWithLargestArmy.GameData.Resources.KnightsPlayed)
            {
                playerWithLargestArmy.GameData.LargestArmy = false;
                controller.CurrentPlayer.GameData.LargestArmy = true;
            }
        }
        public void UndoSetLargestArmy(IGameController controller)
        {
            if (controller.CurrentPlayer.GameData.LargestArmy && controller.CurrentPlayer.GameData.Resources.KnightsPlayed < 3)
            {
                controller.CurrentPlayer.GameData.LargestArmy = false;
                return;
            }

            if (this.BaronModel.PreviousLargestArmyPlayerId == Guid.Empty)
                return;
            var previous = controller.PlayerFromId(this.BaronModel.PreviousLargestArmyPlayerId);
            var current = GetLargestArmyPlayer(controller);
            current.GameData.LargestArmy = false;  //current might be previous, but that is ok
            previous.GameData.LargestArmy = true;
        }

        public async Task Do(IGameController gameController)
        {
            PlayerModel targetPlayer = null;
            var weapon = this.BaronModel.Weapon;
            var targetTile = gameController.TileFromIndex(this.BaronModel.TargetTile);
            if (BaronModel.Victims != null)
            {
                foreach (var victim in this.BaronModel.Victims)
                {
                    targetPlayer = gameController.NameToPlayer(victim);

                    if (targetPlayer != null)
                    {
                        targetPlayer.GameData.TimesTargeted++;
                        if (BaronModel.StolenResource != ResourceType.None)
                        {
                            TradeResources tr = new TradeResources();
                            tr.AddResource(BaronModel.StolenResource, 1);
                            //
                            //  this needs to be face down and not show up in the Turn Resources, but it should count towards Total Resource
                            targetPlayer.GameData.Resources.GrantResources(tr.GetNegated(), false);        // I Taketh
                            gameController.CurrentPlayer.GameData.Resources.GrantResources(tr, false);     // I Giveth

                            targetPlayer.GameData.Resources.StolenResource = BaronModel.StolenResource;
                            gameController.CurrentPlayer.GameData.Resources.StolenResource = BaronModel.StolenResource;

                        }
                    }
                }
            }


            // if they played a dev card, consume it
            // 10/23/2023: we weren't counting knights played this fix will says "if it is a local game,
            //             give them a knight and then let them play it.
            if (BaronModel.Reason == MoveBaronReason.PlayedDevCard)
            {
                if (!gameController.IsServiceGame)
                {
                    DevCardModel model = new DevCardModel() { DevCardType = DevCardType.Knight };
                    gameController.CurrentPlayer.GameData.Resources.AvailableDevCards.Add(model);
                }
                var ret = gameController.CurrentPlayer.GameData.Resources.PlayDevCard(DevCardType.Knight);
                Contract.Assert(ret, "A knight was not found in AvailableDevCards");
                gameController.CurrentPlayer.GameData.Resources.KnightsPlayed++;
                SetLargestArmy(gameController);

            }
            //
            //  this will move the weapon in the UI

            gameController.SetBaronTile(weapon, targetTile, true);

            //
            //    consume the entitlement - this is there for both rolling 7 and playing a Baron Dev Card

            gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.MoveBaron);


            await Task.Delay(0);
        }

        public Task Replay(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {

            var previousTile = gameController.TileFromIndex(this.BaronModel.PreviousTile);
            Debug.Assert(previousTile != null);
            var weapon = this.BaronModel.Weapon;
            foreach (var victim in this.BaronModel?.Victims ?? Enumerable.Empty<string>())
            {

                PlayerModel targetPlayer = gameController.NameToPlayer(victim);
                if (targetPlayer != null)
                {
                    targetPlayer.GameData.TimesTargeted--;
                }


                if (BaronModel.StolenResource != ResourceType.None)
                {
                    TradeResources tr = new TradeResources();
                    tr.AddResource(BaronModel.StolenResource, 1);
                    targetPlayer.GameData.Resources.GrantResources(tr, false);                                  // I giveth back
                    gameController.CurrentPlayer.GameData.Resources.GrantResources(tr.GetNegated(), false);     // I taketh away
                }
            }

            gameController.SetBaronTile(weapon, previousTile, BaronModel.MainBaronHidden);


            if (BaronModel.Reason == MoveBaronReason.PlayedDevCard)
            {

                gameController.CurrentPlayer.GameData.Resources.UndoPlayDevCard(DevCardType.Knight);
                gameController.CurrentPlayer.GameData.Resources.KnightsPlayed--;
            }

            UndoSetLargestArmy(gameController);


            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.MoveBaron);


            await Task.Delay(0);
        }

        #endregion Methods
    }
}