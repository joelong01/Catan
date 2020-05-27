﻿
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Unlike previous strategies, this records only the roll and relies on the state of the game to calculate the consequences of that roll.
    ///     1. get and store the random gold tiles
    /// </summary>
    public class WaitingForRollToWaitingForNext : LogHeader, ILogController
    {
        public WaitingForRollToWaitingForNext() : base()
        {

        }

        public List<RollModel> Rolls { get; set; } = new List<RollModel>();

        internal static async Task PostLog(IGameController gameController, List<RollModel> rolls)
        {

            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRoll);

            WaitingForRollToWaitingForNext logHeader = new WaitingForRollToWaitingForNext()
            {
                CanUndo = true,
                Rolls = rolls,
                Action = CatanAction.ChangedState,
                OldState = GameState.WaitingForRoll,
                NewState = GameState.WaitingForNext,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            Contract.Assert(this.NewState == GameState.WaitingForRoll); // log gets pushed *after* this call

            PlayerModel sentBy = gameController.NameToPlayer(this.SentBy);
            Contract.Assert(sentBy != null);
            RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRolls(this.Rolls);
            Contract.Assert(pickedRoll != null);
            Contract.Assert(pickedRoll.DiceOne > 0 && pickedRoll.DiceOne < 7);
            Contract.Assert(pickedRoll.DiceTwo > 0 && pickedRoll.DiceTwo < 7);




            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }
    }
}
