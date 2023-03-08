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
        ///     a Roll has come in to this machine -- it can come from any machine (including this one)
        ///     update *all* players
        ///
        ///     by the time we get here, the Log has been pushed, so the state is correct.
        ///     The Roll has also been pushed
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

            //
            //  get the player and make sure they are playing
            PlayerModel sentBy = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(sentBy != null);
            Contract.Assert(this.RollState.Rolls != null);

            //
            //  what roll did the user pick?
            //  make sure it is valid, otherwise there is a bug
            RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRoll(this.RollState.Rolls);
            Contract.Assert(pickedRoll != null);
            Contract.Assert(pickedRoll.DiceOne > 0 && pickedRoll.DiceOne < 7);
            Contract.Assert(pickedRoll.DiceTwo > 0 && pickedRoll.DiceTwo < 7);

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
                List<PlayerModel> playersWithTooManyCards = new List<PlayerModel>();
                // 3/8/2023 -- looks like there was a bad fix and we always move the baron.
                //          splitting this clause out of the "if" above -- so if they rolled a 7...if it is a service game...
                if (gameController.MyTurn && gameController.IsServiceGame)
                {


                    foreach (var player in gameController.PlayingPlayers)
                    {
                        if (player.GameData.Resources.TotalResourcesForGame.Count > 7)
                        {
                            // at least one person has more than 7 cards and needs to get rid of 1/2 of them.
                            playersWithTooManyCards.Add(player);

                        }
                    }


                    if (playersWithTooManyCards.Count > 0)
                    {
                        //
                        //  somebody has too many cards.  pop a dialog box telling everybody indicating which player(s) need(s) to discard
                        //  

                        string csv = gameController.PlayerListToCsv(playersWithTooManyCards);
                        string msg = "";

                        if (playersWithTooManyCards.Count == 1)
                        {
                            msg = $"{csv} has too many cards and must discard 1/2 of them before we continue.";
                        }
                        else if (playersWithTooManyCards.Count > 1)
                        {
                            msg = $"{csv} all have too many cards and must discard 1/2 of them before we continue.";
                        }

                        ContentDialog dlg = new ContentDialog()
                        {
                            Title = "Catan - Rolled 7",
                            Content = msg,
                            CloseButtonText = "Ok",
                        };

                        await dlg.ShowAsync();

                        await PlayerHasTooManyCards.PostMessage(gameController);
                    }
                }
                else
                {
                    // 
                    //  we transition out of this state above and then return.  if we get through it, we know that 
                    //  nobody has too many cards -- so now we can move the Baron
                    //

                    await MustMoveBaronLog.PostLog(gameController, MoveBaronReason.Rolled7);
                }
            }

        }

        public async Task Replay(IGameController gameController)
        {

            //
            //  get the player and make sure they are playing
            PlayerModel sentBy = gameController.NameToPlayer(this.SentBy);

            //
            //  what roll did the user pick?
            //  make sure it is valid, otherwise there is a bug
            RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRoll(this.RollState.Rolls);

            IRollLog rollLog = gameController.RollLog;

            //
            //  this pushes the rolls onto the roll stack and updates all the data for the roll
            //
            await gameController.Log.RollLog.UpdateUiForRoll(this.RollState);

            //
            //  TODO: This also needs to do somethign different if it is the last message -- e.g. call Do();
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        { //
            // if the state is wrong, there is a big bug someplace
            Contract.Assert(this.NewState == GameState.WaitingForNext); // log gets pushed *after* this call

            //
            //  get the player and make sure they are playing
            PlayerModel sentBy = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(sentBy != null);

            //
            //  what roll did the user pick?
            //  make sure it is valid, otherwise there is a bug
            RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRoll(this.RollState.Rolls);
            Contract.Assert(pickedRoll != null);
            Contract.Assert(pickedRoll.DiceOne > 0 && pickedRoll.DiceOne < 7);
            Contract.Assert(pickedRoll.DiceTwo > 0 && pickedRoll.DiceTwo < 7);

            await gameController.Log.RollLog.UndoRoll();
        }

        internal static async Task PostRollMessage(IGameController gameController, List<RollModel> rolls)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRoll);
            var rollState = gameController.Log.RollLog.Peek();
            Contract.Assert(rolls.Count > 0);
            Contract.Assert(rollState != null);
            rollState.Rolls = rolls;
            rollState.SelectedRoll = rolls.Find((roll) => roll.Selected == true).Roll;

            WaitingForRollToWaitingForNext logHeader = new WaitingForRollToWaitingForNext()
            {
                CanUndo = false,
                RollState = rollState,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForNext,
            };

            if (logHeader.RollState.Rolls != null)
            {
                Contract.Assert(logHeader.RollState.Rolls.Count == rolls.Count);
                logHeader.TraceMessage("Find a better way to make sure the rolls are the same");
            }

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        #endregion Methods
    }
}