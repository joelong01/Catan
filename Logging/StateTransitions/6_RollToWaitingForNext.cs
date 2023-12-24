using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;
using Windows.UI.Xaml.Controls;

namespace Catan10
{
    /// <summary>
    ///     Unlike previous strategies, this records only the roll and relies on the state of the game to calculate the consequences of that roll.
    ///
    ///     The RollState is *must be* the top of the RollLog stack
    /// </summary>
    public class WaitingForRollToWaitingForNext : LogHeader, ILogController
    {
        #region Properties

        public RollState RollState { get; set; } = null;

        #endregion Properties

        #region Constructors + Destructors

        public WaitingForRollToWaitingForNext() : base()
        {
        }

        #endregion Constructors + Destructors

        #region Methods

        /// <summary>
        ///    a Roll has been entered via clicking on the roll in the UI
        ///
        ///     the UI for the Roll has not been updated - need to call that here.
        ///
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public async Task Do(IGameController gameController)
        {
            //
            // if the state is wrong, there is a big bug someplace
            Contract.Assert(this.NewState == GameState.WaitingForNext); // log gets pushed *after* this call


            IRollLog rollLog = gameController.RollLog;

            //
            //  this pushes the rolls onto the roll stack and updates all the data for the roll
            //
            await gameController.Log.RollLog.UpdateUiForRoll(this.RollState);

            //
            //  does anybody have any gold?
            //  this message goes everywhere, so we only need one machien to send it. so even if 2 people need to trade gold, only one sends it
            //
            //  7/14/2021: don't send this message for local games as we don't use the gameui to trade gold in local games
            //
            if (gameController.TheHuman == gameController.CurrentPlayer && gameController.IsServiceGame)
            {
                // current player checks each player to see if anybody has gold
                foreach (var player in gameController.PlayingPlayers)
                {
                    if (player.GameData.Resources.CurrentResources.GoldMine > 0)
                    {
                        // at least one person has gold
                        await MustTradeGold.PostMessage(gameController);
                        break;
                    }
                }
            }


            // only the current player's machine should send this message.  everybody will get it
            //
            //  7/14/2021:  only count # cards in a hand on service games.  by not filling in the card count, the list will be length 0 for the check below
            if (rollLog.LastRoll == 7)
            {
                await gameController.RolledSeven();
                
                
                
            }

        }

        public async Task Replay(IGameController gameController)
        {

            //
            //  this pushes the rolls onto the roll stack and updates all the data for the roll
            //
            await gameController.Log.RollLog.UpdateUiForRoll(this.RollState);

            //
            //  TODO: This also needs to do somethign different if it is the last message -- e.g. call Do();
        }

        public async Task Redo(IGameController gameController)
        {
            await gameController.Log.RollLog.RedoRoll();
        }

        public async Task Undo(IGameController gameController)
        { //
            // if the state is wrong, there is a big bug someplace
            Contract.Assert(this.NewState == GameState.WaitingForNext); // log gets pushed *after* this call
            await gameController.Log.RollLog.UndoRoll();
        }
        //
        //  the rollstate is pushed to the look
        internal static async Task PostRollMessage(IGameController gameController, int roll)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRoll);
            var rollState = gameController.Log.RollLog.Peek();
          
            rollState.Roll = roll;
         
            WaitingForRollToWaitingForNext logHeader = new WaitingForRollToWaitingForNext()
            {
                CanUndo = true,
                RollState = rollState,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForNext,
            };


            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        #endregion Methods
    }
}