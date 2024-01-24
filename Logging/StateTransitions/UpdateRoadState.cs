using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     This class has all the data associated with a updating a road

    /// </summary>
    public class UpdateRoadLog : LogHeader, ILogController
    {
     
        public RoadState NewRoadState { get; set; } = RoadState.Unowned;
        public RoadState OldRoadState { get; set; } = RoadState.Unowned;
        public int RoadIndex { get; set; } = -1;
        public Guid RoadOwner { get; set; } = Guid.Empty;

        public static async Task PostLogEntry(IGameController gameController, RoadCtrl road, RoadState newRoadState)
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
                RoadOwner = (road.Owner == null) ? Guid.Empty : road.Owner.PlayerIdentifier
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            RoadCtrl road = gameController.GetRoad(RoadIndex);
            Contract.Assert(road != null);
            var owner = gameController.PlayerFromId(RoadOwner);
            var sentBy = gameController.CurrentPlayer;
            road.RoadState = NewRoadState;
            switch (NewRoadState)
            {
                case RoadState.Unowned: // this happens with Diplomat being played
                    if (OldRoadState == RoadState.Ship)
                    {
                        Debug.Assert(owner != null);
                        owner.GameData.Ships.Remove(road);
                      //  sentBy.GameData.Resources.GrantEntitlement(Entitlement.Ship);
                    }
                    else
                    {
                        Debug.Assert(owner != null);
                        owner.GameData.Roads.Remove(road);
                       // owner.GameData.Resources.GrantEntitlement(Entitlement.Road);
                    }

                    road.Owner = null;
                    break;

                case RoadState.Road:
                    sentBy.GameData.Resources.ConsumeEntitlement(Entitlement.Road);
                    sentBy.GameData.Roads.Add(road);
                    road.Owner = sentBy;
                    break;

                case RoadState.Ship:
                    sentBy.GameData.Resources.ConsumeEntitlement(Entitlement.Ship);
                    sentBy.GameData.Roads.Remove(road); // can't be a ship if you aren't a road
                    sentBy.GameData.Ships.Add(road);
                    break;

                default:
                    break;
            }


            await LongestRoadChangedLog.CalculateAndSetLongestRoad(gameController);
            await DefaultTask;
        }

        public static PlayerModel LongestRoadPlayer(List<PlayerModel> players)
        {
            foreach (var p in players)
            {
                if (p.GameData.HasLongestRoad) return p;
            }

            return null;
        }

        public Task Replay(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }
        //
        // transitions back from Road or Ship to Unowned and updates the
        // datastructures.  does nothing with longest road as this will
        // be done via the LongestRoadChanged log entry
        public async Task Undo(IGameController gameController)
        {
            RoadCtrl road = gameController.GetRoad(RoadIndex);
            Debug.Assert(road != null);
            var owner = gameController.PlayerFromId(RoadOwner);
            var sentBy = gameController.CurrentPlayer;
            if (owner == null) owner = sentBy;
            road.RoadState = OldRoadState;
            switch (NewRoadState)
            {
                case RoadState.Unowned:
                    road.Owner = owner;
                 
                    if (OldRoadState == RoadState.Ship)
                    {
                        owner.GameData.Ships.Add(road);
                    } else
                    {
                        owner.GameData.Roads.Add(road);
                    }
                    break;
                case RoadState.Road:
                    owner.GameData.Roads.Remove(road);
                    sentBy.GameData.Resources.GrantEntitlement(Entitlement.Road);
                    road.Owner = null;
                    break;
                case RoadState.Ship:
                    owner.GameData.Ships.Remove(road);
                    sentBy.GameData.Resources.GrantEntitlement(Entitlement.Ship);
                    road.Owner = null;
                    break;
                default:
                    break;
            }
            

            await DefaultTask;
        }
    }
}