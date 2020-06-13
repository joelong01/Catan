﻿using System;
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

        public static async Task PostLog(IGameController gameController, PlayerModel victim, int targetTileIndex, int previousIndex, TargetWeapon weapon, MoveBaronReason reason, ResourceType stolenResource)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.MustMoveBaron);
            GameState previousState = gameController.MainPageModel.Log.PeekAction.OldState;
            Debug.Assert(((MustMoveBaronLog)gameController.MainPageModel.Log.PeekAction).StartingState == previousState);

            MovedBaronLog logHeader = new MovedBaronLog()
            {
                CanUndo = true,
                Action = CatanAction.MovingBaron,
                NewState = previousState,
                BaronModel = new BaronModel()
                {
                    Victim = (victim == null) ? "" : victim.PlayerName,
                    Weapon = weapon,
                    PreviousTile = previousIndex,
                    TargetTile = targetTileIndex,
                    StolenResource = stolenResource,
                    Reason = reason
                }
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            PlayerModel targetPlayer = null;
            if (this.BaronModel.Victim != null && this.BaronModel.Victim != "")
            {
                targetPlayer = gameController.NameToPlayer(this.BaronModel.Victim);
            }

            var targetTile = gameController.TileFromIndex(this.BaronModel.TargetTile);
            var weapon = this.BaronModel.Weapon;

            if (targetPlayer != null)
            {
                targetPlayer.GameData.TimesTargeted++;
                if (BaronModel.StolenResource != ResourceType.None)
                {
                    TradeResources tr = new TradeResources();
                    tr.Add(BaronModel.StolenResource, 1);
                    //
                    //  this needs to be face down and not show up in the Turn Resources, but it shoudl count towards Total Resource
                    targetPlayer.GameData.Resources.GrantResources(tr.GetNegated());        // I Taketh
                    gameController.CurrentPlayer.GameData.Resources.GrantResources(tr);     // I Giveth

                    //
                    //  need a way to show the victim what card they lost...
                    //
                }
            }

            // if they played a dev card, consume it
            if (BaronModel.Reason == MoveBaronReason.PlayedDevCard)
            {
                var ret = gameController.CurrentPlayer.GameData.Resources.PlayDevCard(DevCardType.Knight);
                Contract.Assert(ret, "A knight was not found in AvailableDevCards");
                gameController.CurrentPlayer.GameData.Resources.KnightsPlayed++;
                
            }


            //
            //  this will move the weapon in the UI
            if (weapon == TargetWeapon.PirateShip)
            {
                gameController.GameContainer.PirateShipTile = targetTile;
            }
            else
            {
                gameController.GameContainer.BaronTile = targetTile;
            }

            return Task.CompletedTask;
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

            if (targetPlayer != null)
            {
                targetPlayer.GameData.TimesTargeted--;
            }

            if (weapon == TargetWeapon.PirateShip)
            {
                gameController.GameContainer.PirateShipTile = previousTile;
                if (BaronModel.StolenResource != ResourceType.None)
                {
                    TradeResources tr = new TradeResources();
                    tr.Add(BaronModel.StolenResource, 1);
                    targetPlayer.GameData.Resources.GrantResources(tr);                                  // I giveth back
                    gameController.CurrentPlayer.GameData.Resources.GrantResources(tr.GetNegated());     // I taketh away
                }
            }
            else
            {
                gameController.GameContainer.BaronTile = previousTile;
                if (BaronModel.StolenResource != ResourceType.None)
                {
                    TradeResources tr = new TradeResources();
                    tr.Add(BaronModel.StolenResource, 1);
                    targetPlayer.GameData.Resources.GrantResources(tr);                                  // I giveth back
                    gameController.CurrentPlayer.GameData.Resources.GrantResources(tr.GetNegated());     // I taketh away
                }
            }

            // if they played a dev card, undo it
            if (BaronModel.Reason == MoveBaronReason.PlayedDevCard)
            {
                
                gameController.CurrentPlayer.GameData.Resources.UndoPlayDevCard(DevCardType.Knight);                
                gameController.CurrentPlayer.GameData.Resources.KnightsPlayed--;

            }

            gameController.CurrentPlayer.GameData.MovedBaronAfterRollingSeven = false;

            return Task.CompletedTask;
        }

        #endregion Methods
    }
}