using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10.Tests
{
    internal class KnightDisplacementTest
    {

        IGameController Controller { get; set; }
        public KnightDisplacementTest(IGameController controller)
        {
            Controller = controller;
        }

        public async Task TestKnightDisplacement()
        {
            //
            //  all of these arrays are in reverse order because we use them to initialize a stack and Pop() takes off from the end of the array
            var player0Buildings = new int[] {18, 29, 27, 6};
            var player1Buildings = new int[] {14, 41, 16};
            var player2Buildings = new int[] {30, 43, 31};


            var player0Roads = new int[] {37, 22, 36, 23};
            var player1Roads = new int[] {17,18,54, 21};
            var player2Roads = new int[] {56, 39};
           

            var roadAndBuildings = new List<(Stack<int> buildings, Stack<int> roads)>() {
                (new Stack<int>(player0Buildings), new Stack<int>(player0Roads)),
                (new Stack<int>(player1Buildings), new Stack<int>(player1Roads)),
                (new Stack<int>(player2Buildings), new Stack<int>(player2Roads)),

            };

            var rank2Knights = new List<int>() {29, 14};

            GameInfo info = new GameInfo()
            {
                Creator = Controller.MainPageModel.AllPlayers[0].PlayerName,
                GameIndex = 0,
                Id = Guid.NewGuid(),
                Started = false,
                CitiesAndKnights=true
            };

            await Controller.StartTestGame(info, autoSetResources: false);
            var testHelper = new TestHelper(Controller);
            await testHelper.StartGame(roadAndBuildings);


            int roadIndex;
            int playerIndex;
            int knightIndex;

            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForRoll);
            for (int i = 0; i < Controller.PlayingPlayers.Count; i++)
            {
                await Controller.Test_DoRoll(3, 5, SpecialDice.Science);
                playerIndex = Controller.PlayingPlayers.IndexOf(Controller.CurrentPlayer);
                Debug.Assert(Controller.CurrentGameState == GameState.WaitingForNext);

                // build all roads for player 
                while (roadAndBuildings[playerIndex].roads.Count > 0)
                {
                    roadIndex = roadAndBuildings[playerIndex].roads.Pop();
                    await Controller.PurchaseAndPlaceRoad(roadIndex);

                }
                await Task.Delay(100);
                while (roadAndBuildings[playerIndex].buildings.Count > 0)
                {
                    knightIndex = roadAndBuildings[playerIndex].buildings.Pop();
                    KnightRank rank = KnightRank.Basic;
                    if (rank2Knights.Contains(knightIndex)) rank = KnightRank.Strong;
                    await Controller.PurchaseAndPlaceKnight(knightIndex: knightIndex, activate: true, rank: rank);
                    await Task.Delay(10);
                }

                await Controller.NextState();
            }
            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForRoll);

            await Controller.Test_DoRoll(3, 5, SpecialDice.Science);

            await TestCheckpointLog.AddTestCheckpoint(this.Controller);

            //
            //  first Displace the knight that can't go anywhere

            await Controller.PurchaseEntitlement(Entitlement.KnightDisplacement); // if you have a bug, this shows UI...
            Debug.Assert(Controller.CurrentPlayer.GameData.Resources.UnspentEntitlements.Contains(Entitlement.KnightDisplacement));
            var aggressor = Controller.GetBuilding(29);
            var targets =  aggressor.GetConnectedBuildings(DropTargetOptions.Knights);
            Debug.Assert(targets.Count == 1);
            Debug.Assert(targets[0].Owner == Controller.PlayingPlayers[2]);
            await DisplaceKnightLog.DisplaceKnightPhaseOne(Controller, aggressor, targets[0]);
            Debug.Assert(targets[0].BuildingState == BuildingState.Knight);
            Debug.Assert(targets[0].Owner == Controller.CurrentPlayer);
            Debug.Assert(targets[0].Knight.Activated == false);
            Debug.Assert(Controller.PlayingPlayers[2].GameData.CK_Knights.Count == 0);
            Debug.Assert(Controller.MainPageModel.TotalKnightRanks == 3);
            await Controller.RollbackToCheckpoint();

            // displace a knight that only has one place to go
            await TestCheckpointLog.AddTestCheckpoint(this.Controller);
            Debug.Assert(Controller.MainPageModel.TotalKnightRanks == 6); // player 2's knight + player 0's knight now active
            await Controller.NextState();
            Debug.Assert(Controller.CurrentPlayer == Controller.PlayingPlayers[1]);
            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForRoll);
            await Controller.Test_DoRoll(3, 5, SpecialDice.Science);
            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForNext);
            await Controller.PurchaseEntitlement(Entitlement.KnightDisplacement);
            aggressor = Controller.GetBuilding(14);
            targets = aggressor.GetConnectedBuildings(DropTargetOptions.Knights);
            Debug.Assert(targets.Count == 1);
            Debug.Assert(targets[0].Index == 18);
            await DisplaceKnightLog.DisplaceKnightPhaseOne(Controller, aggressor, targets[0]);
            Debug.Assert(Controller.GetBuilding(19).Knight.Owner == Controller.PlayingPlayers[0]);
            Debug.Assert(Controller.GetBuilding(19).Knight.Activated);
            Debug.Assert(Controller.GetBuilding(18).Knight.Owner == Controller.PlayingPlayers[1]);
            Debug.Assert(Controller.GetBuilding(18).Knight.Activated == false);
            Debug.Assert(Controller.GetBuilding(18).Knight.KnightRank == KnightRank.Strong);
            Debug.Assert(Controller.MainPageModel.TotalKnightRanks == 4);
            await Controller.RollbackToCheckpoint();
            await TestCheckpointLog.AddTestCheckpoint(this.Controller);

            // displace a knight with a choice where to go
            Debug.Assert(Controller.CurrentPlayer == Controller.PlayingPlayers[0]);
            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForNext);
            await Controller.PurchaseAndPlaceRoad(24);
            await Controller.NextState();
            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForRoll);
            Debug.Assert(Controller.CurrentPlayer == Controller.PlayingPlayers[1]);


            await TestCheckpointLog.AddTestCheckpoint(this.Controller);
            Debug.Assert(Controller.MainPageModel.TotalKnightRanks == 6); 
        
            await Controller.Test_DoRoll(3, 5, SpecialDice.Science);

            await Controller.PurchaseEntitlement(Entitlement.KnightDisplacement);
            aggressor = Controller.GetBuilding(14);
            targets = aggressor.GetConnectedBuildings(DropTargetOptions.Knights);
            Debug.Assert(targets.Count == 1);
            Debug.Assert(targets[0].Index == 18);
            await DisplaceKnightLog.DisplaceKnightPhaseOne(Controller, aggressor, targets[0]);
            var openSpots = targets[0].GetConnectedBuildings(DropTargetOptions.OpenBuildings);
            Debug.Assert(openSpots.Contains(Controller.GetBuilding(20)));
            Debug.Assert(openSpots.Count == 2);
            await DisplaceKnightLog.DisplaceKnightPhaseTwo(Controller, targets[0], Controller.GetBuilding(20));
            Debug.Assert(Controller.GetBuilding(20).Knight.Owner == Controller.PlayingPlayers[0]);
            Debug.Assert(Controller.GetBuilding(20).Knight.Activated);
            Debug.Assert(Controller.GetBuilding(18).Knight.Owner == Controller.PlayingPlayers[1]);
            Debug.Assert(Controller.GetBuilding(18).Knight.Activated == false);
            Debug.Assert(Controller.GetBuilding(18).Knight.KnightRank == KnightRank.Strong);
          // await Controller.RollbackToCheckpoint();

        }
    }
}
