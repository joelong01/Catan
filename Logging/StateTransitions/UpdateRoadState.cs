using System;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     This class has all the data associated with a updating a road

    /// </summary>
    public class UpdateRoadLog : LogHeader, ILogController
    {
        public RoadRaceTracking NewRaceTracking { get; set; } = new RoadRaceTracking(MainPage.Current);

        public RoadState NewRoadState { get; set; } = RoadState.Unowned;

        public RoadRaceTracking OldRaceTracking { get; set; } = new RoadRaceTracking(MainPage.Current);

        public RoadState OldRoadState { get; set; } = RoadState.Unowned;

        public int RoadIndex { get; set; } = -1;

        public UpdateRoadLog() : base()
        {
        }

        public static async Task SetRoadState(IGameController gameController, RoadCtrl road, RoadState newRoadState, RoadRaceTracking raceTracker)
        {
            RoadState oldState = road.RoadState;

            if (newRoadState == oldState)
            {
                throw new Exception("Why are you updating the road state to be the same state it already is?");
            }

            UpdateRoadLog logHeader = new UpdateRoadLog()
            {
                Action = CatanAction.UpdatedRoadState,
                RoadIndex = road.Index,
                OldRoadState = road.RoadState,
                NewRoadState = newRoadState,
                OldRaceTracking = raceTracker,
                SentBy = gameController.CurrentPlayer.PlayerName
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return gameController.SetRoadState(this);
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.SetRoadState(this);
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.UndoSetRoadState(this);
        }
    }
}