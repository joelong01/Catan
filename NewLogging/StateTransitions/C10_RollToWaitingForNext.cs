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
        #region Methods

        internal static async Task PostLog(IGameController gameController, List<RollModel> rolls)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRoll);

            WaitingForRollToWaitingForNext logHeader = new WaitingForRollToWaitingForNext()
            {
                CanUndo = true,
                GoldTiles = gameController.CurrentRandomGoldTiles,
                Rolls = rolls,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForNext,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        #endregion Methods

        #region Constructors

        public WaitingForRollToWaitingForNext() : base()
        {
        }

        #endregion Constructors

        #region Properties

        public List<int> GoldTiles { get; set; }
        public List<RollModel> Rolls { get; set; } = new List<RollModel>();

        #endregion Properties

        // set in Do() prior to logging because we need to Push the roll before we get the Tiles
        /// <summary>
        ///     a Roll has come in to this machine -- it can come from any machine (including this one)
        ///     update *all* players
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

            //
            //  what roll did the user pick?
            //  make sure it is valid, otherwise there is a bug
            RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRolls(this.Rolls);
            Contract.Assert(pickedRoll != null);
            Contract.Assert(pickedRoll.DiceOne > 0 && pickedRoll.DiceOne < 7);
            Contract.Assert(pickedRoll.DiceTwo > 0 && pickedRoll.DiceTwo < 7);

            IRollLog rollLog = gameController.RollLog;

            //
            //  this pushes the rolls onto the roll stack and updates all the data for the roll
            //
            await rollLog.DoRoll(this.Rolls, this.GoldTiles);

            if (rollLog.LastRoll == 7)
            {
                await WaitingForNextToMustRollBaron.PostLog(gameController);
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
            RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRolls(this.Rolls);
            Contract.Assert(pickedRoll != null);
            Contract.Assert(pickedRoll.DiceOne > 0 && pickedRoll.DiceOne < 7);
            Contract.Assert(pickedRoll.DiceTwo > 0 && pickedRoll.DiceTwo < 7);

            await gameController.Log.RollLog.UndoRoll();
        }
    }
}
