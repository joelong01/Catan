using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Need to get the random gold tiles for this turn
    /// </summary>
    public class DoneAllocResourcesToWaitingForRoll : LogHeader, ILogController, IMessageDeserializer
    {
        internal static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.DoneResourceAllocation);

            DoneAllocResourcesToWaitingForRoll logHeader = new DoneAllocResourcesToWaitingForRoll()
            {
                CanUndo = true,
                RollState = gameController.GetNextRollState(),
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForRoll,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public RollState RollState { get; set; } = null;

        // this is what the JSON gets Deserialized to

        public LogHeader Deserialize(string json)
        {
            DoneAllocResourcesToWaitingForRoll logHeader = CatanProxy.Deserialize<DoneAllocResourcesToWaitingForRoll>(json);
            return logHeader;
        }

        public async Task Do(IGameController gameController)
        {
            Contract.Assert(this.RollState.PlayerName == gameController.CurrentPlayer.PlayerName);

            //
            //  hide the rolls in the public data control
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.Resources.ResourcesThisTurn = new TradeResources(); // you don't have to set Orientation because setting Count to 0 sets orientation = facedown
                p.GameData.Resources.ResourcesThisTurn2.Reset();
            });

            

            await gameController.PushRollState(RollState); // also set RandomGold tiles in the UI

            //
            // if we have a roll for this turn already, use it.
            if (RollState.Rolls != null && RollState.Rolls.Count > 0)
            {
                await WaitingForRollToWaitingForNext.PostRollMessage(gameController, RollState.Rolls);
            }
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            await gameController.ResetRandomGoldTiles();
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.Resources.ResourcesThisTurn += p.GameData.Resources.Current;
            });
        }
    }
}