using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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

        public static async Task PostLog(IGameController gameController, List<string> victims, int targetTileIndex, int previousIndex, TargetWeapon weapon, MoveBaronReason reason, ResourceType stolenResource)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.MustMoveBaron);
            GameState newState = gameController.MainPageModel.Log.PeekAction.OldState;
            if (newState == GameState.TooManyCards)
            {
                newState = GameState.WaitingForNext;
            }

            // Debug.Assert(((MustMoveBaronLog)gameController.MainPageModel.Log.PeekAction).StartingState == previousState);

            MovedBaronLog logHeader = new MovedBaronLog()
            {
                CanUndo = true,
                Action = CatanAction.MovingBaron,
                NewState = newState,
                BaronModel = new BaronModel()
                {
                    Victims = victims,
                    Weapon = weapon,
                    PreviousTile = previousIndex,
                    TargetTile = targetTileIndex,
                    StolenResource = stolenResource,
                    Reason = reason,
                    MainBaronHidden = gameController.BaronVisibility
                }
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            PlayerModel targetPlayer = null;
            var weapon = this.BaronModel.Weapon;
            var targetTile = gameController.TileFromIndex(this.BaronModel.TargetTile);

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
                    gameController.AssignLargestArmy();
                }

            }


            //
            //  this will move the weapon in the UI

            gameController.SetBaronTile(weapon, targetTile, true);



            //
            //  Show the card taken in the UI - but only for the Victim and the player that took the card

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
            var weapon = this.BaronModel.Weapon;

            foreach (var victim in this.BaronModel.Victims)
            {

                PlayerModel targetPlayer = gameController.NameToPlayer(victim);
                if (targetPlayer != null)
                {
                    targetPlayer.GameData.TimesTargeted--;
                }



                gameController.SetBaronTile(weapon, previousTile, this.BaronModel.MainBaronHidden);
                if (BaronModel.StolenResource != ResourceType.None)
                {
                    TradeResources tr = new TradeResources();
                    tr.AddResource(BaronModel.StolenResource, 1);
                    targetPlayer.GameData.Resources.GrantResources(tr, false);                                  // I giveth back
                    gameController.CurrentPlayer.GameData.Resources.GrantResources(tr.GetNegated(), false);     // I taketh away
                }


                // if they played a dev card, undo it if it is a service game (local games don't track resources)
                if (BaronModel.Reason == MoveBaronReason.PlayedDevCard && gameController.IsServiceGame)
                {

                    gameController.CurrentPlayer.GameData.Resources.UndoPlayDevCard(DevCardType.Knight);

                }

                // 10/23/2023: this was the corresponding fix to bug above -- we need to decrement knight count on undo, for all game types
                if (this.BaronModel.Reason != MoveBaronReason.Bishop)
                {
                    gameController.CurrentPlayer.GameData.Resources.KnightsPlayed--;
                    gameController.AssignLargestArmy();
                }
            }

            await Task.Delay(0);
        }

        #endregion Methods
    }
}