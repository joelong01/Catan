using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Catan.Proxy;

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

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
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
                await WaitingForNextToMustMoveBaron.PostLog(gameController);
                //
                //  need to update statistics
                //  need to provide a way to take a player's card
                //  go through all the players and if the have > 7 resources, provide a way to give 1/2 to the bank
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
