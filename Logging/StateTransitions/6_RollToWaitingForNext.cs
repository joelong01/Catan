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
    public class WaitingForRollToWaitingForNext : LogHeader, ILogController, IMessageDeserializer
    {
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
                CanUndo = true,
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

        public RollState RollState { get; set; } = null;

        public WaitingForRollToWaitingForNext() : base()
        {
        }

        public LogHeader Deserialize(string json)
        {
            WaitingForRollToWaitingForNext logHeader = CatanProxy.Deserialize<WaitingForRollToWaitingForNext>(json);
            return logHeader;
        }

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
            RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRolls(this.RollState.Rolls);
            Contract.Assert(pickedRoll != null);
            Contract.Assert(pickedRoll.DiceOne > 0 && pickedRoll.DiceOne < 7);
            Contract.Assert(pickedRoll.DiceTwo > 0 && pickedRoll.DiceTwo < 7);

            IRollLog rollLog = gameController.RollLog;

            //
            //  this pushes the rolls onto the roll stack and updates all the data for the roll
            //
            await gameController.Log.RollLog.UpdateUiForRoll(this.RollState);

            if (rollLog.LastRoll == 7)
            {
                //
                //  we explicity discard before we target! -- this is *not* about the Baron, so don't move it.
                //  this is about having too many cards
                //  

                if (gameController.TheHuman.GameData.Resources.Current.Count > 7)
                {
                    gameController.TheHuman.GameData.Resources.ResourcesThisTurn = new TradeResources();
                    int loss = (int)gameController.TheHuman.GameData.Resources.Current.Count / 2;
                    TradeResources lost = new TradeResources();
                    ResourceCardCollection source = new ResourceCardCollection();
                    source.InitalizeResources(gameController.TheHuman.GameData.Resources.Current);
                    TakeCardDlg dlg = new TakeCardDlg()
                    {
                        To = gameController.MainPageModel.Bank,
                        From = gameController.TheHuman,
                        SourceOrientation = TileOrientation.FaceUp,
                        HowMany = loss,
                        Source = source,
                        Destination = new ResourceCardCollection(),
                        Instructions = $"Give {loss} cards to the bank."
                    };
                    var ret = await dlg.ShowAsync();
                    if (ret == ContentDialogResult.Primary)
                    {
                        lost = ResourceCardCollection.ToTradeResources(dlg.Destination);
                    }
                    else
                    {
                        await StaticHelpers.ShowErrorText($"Since you cancelled out of the dialog (I assume it was you Dodgy) the game will now pick {loss} random cards.", "Catan");
                        var list = gameController.TheHuman.GameData.Resources.Current.ToList();
                        Random rand = new Random((int)DateTime.Now.Ticks);

                        for (int i = 0; i < loss; i++)
                        {
                            int index = rand.Next(list.Count);
                            lost.Add(list[index], 1);
                            list.RemoveAt(index);
                        }
                    }

                    Contract.Assert(lost.Count == loss);
                    gameController.TheHuman.GameData.Resources.GrantResources(lost.GetNegated());
                    gameController.TheHuman.GameData.Resources.ResourcesLostSeven += lost;
                }
                //
                //  Now transition to the state
                //

                await MustMoveBaronLog.PostLog(gameController, MoveBaronReason.Rolled7);
                
            }
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
            RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRolls(this.RollState.Rolls);
            Contract.Assert(pickedRoll != null);
            Contract.Assert(pickedRoll.DiceOne > 0 && pickedRoll.DiceOne < 7);
            Contract.Assert(pickedRoll.DiceTwo > 0 && pickedRoll.DiceTwo < 7);

            await gameController.Log.RollLog.UndoRoll();
        }
    }
}