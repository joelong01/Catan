using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;
using Windows.Media.Playback;
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


        public RollState RollState { get; set; } = null;



        public WaitingForRollToWaitingForNext() : base()
        {
        }
        internal static async Task PostRollMessage(IGameController gameController, RollModel roll)
        {
            System.Diagnostics.Debug.Assert(gameController.CurrentGameState == GameState.WaitingForRoll ||
                gameController.CurrentGameState == GameState.HandlePirates ||
                gameController.CurrentGameState == GameState.DoneDestroyingCities);
            var rollState = new RollState()
            {
                RollModel = roll,
                PlayerName = gameController.CurrentPlayer.PlayerName
            };

            bool undoNext = false;
            if (gameController.CurrentGameState == GameState.HandlePirates)
            {
                undoNext = true;
            }

            WaitingForRollToWaitingForNext logHeader = new WaitingForRollToWaitingForNext()
            {
                CanUndo = true,
                RollState = rollState,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForNext,
                UndoNext = undoNext
            };


            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        /// <summary>
        ///    a RollModel has been entered via clicking on the roll in the UI
        ///
        ///     the UI for the RollModel has not been updated - need to call that here.
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
            //  7/14/2021:  only count # cards in a hand on service games.  by not filling in the card count, the list will be length 0 for the check below
            if (rollLog.LastRoll.Roll == 7)
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
            //
            //   if this is a pirate game, handle the pirate roll


        }

        public async Task Undo(IGameController gameController)
        { //
            // if the state is wrong, there is a big bug someplace
            Contract.Assert(this.NewState == GameState.WaitingForNext); // log gets pushed *after* this call
            await gameController.Log.RollLog.UndoRoll();
     
        }
    }

}
