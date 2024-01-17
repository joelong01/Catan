using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    public class TestHelper
    {
        IGameController Controller { get; set; }

        public TestHelper(IGameController controller)
        {
            Controller = controller;
        }

        public async Task StartGame(List<(Stack<int> buildings, Stack<int> roads)> roadAndBuildings)
        {
            Debug.Assert(Controller.CurrentGameState == GameState.AllocateResourceForward);

            bool success;
            int playerIndex;
            int roadIndex;
            int buildingIndex;
            BuildingCtrl building;
            RoadCtrl road;
            while (Controller.CurrentGameState == GameState.AllocateResourceForward || Controller.CurrentGameState == GameState.AllocateResourceReverse)
            {
                var state = Controller.CurrentGameState;
                while (Controller.CurrentGameState == state)
                {
                    playerIndex = Controller.PlayingPlayers.IndexOf(Controller.CurrentPlayer);
                    buildingIndex = roadAndBuildings[playerIndex].buildings.Pop();
                    building = Controller.GetBuilding(buildingIndex);
                    roadIndex = roadAndBuildings[playerIndex].roads.Pop();
                    road = Controller.GetRoad(roadIndex);
                    Debug.Assert(road != null);
                    //
                    //  we don't use the PurchaseAndPlaceBuiding because the entitlement is already granted in the allocation phase
                    await UpdateBuildingLog.UpdateBuildingState(Controller, building, BuildingState.Settlement, Controller.CurrentGameState);
                    if (Controller.CurrentPlayer.GameData.Resources.UnspentEntitlements.Contains(Entitlement.City))
                    {
                        await UpdateBuildingLog.UpdateBuildingState(Controller, building, BuildingState.City, Controller.CurrentGameState);
                    }
                    await UpdateRoadLog.PostLogEntry(Controller, road, RoadState.Road, Controller.RaceTracking);
                    success = await Controller.NextState();
                    Debug.Assert(success);

                }

            }

            for (int i = 0; i < Controller.PlayingPlayers.Count; i++)
            {
                Expectations.Check(Controller, playerIndex: i, roads: 2, longestRoad: 1, hasLongestRoad: false, score: 3);
            }

            success = await Controller.NextState(); // start
            Debug.Assert(success);
            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForRoll);


        }
    }

}