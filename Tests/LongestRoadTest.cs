using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    public static class PlayerExtensions
    {
        public static PlayerExpect Expect(this PlayerModel player)
        {
            return new PlayerExpect() { Player = player };
        }
    }
    public class PlayerExpect
    {
        public PlayerModel Player { get; set; }
        public void LongestRoad(int count, string msg = "")
        {
            Debug.Assert(Player.CalculateLongestRoad() == count, msg);
        }
        public void RoadCount(int count, string msg = "")
        {
            //
            //  assumes all roads for now
            Debug.Assert(Player.GameData.RoadsAndShips.Count == count, msg);
            Debug.Assert(Player.GameData.Roads.Count == count, msg);
        }

        public void HasLongestRoad(bool yes, string msg = "")
        {
            Debug.Assert(Player.GameData.HasLongestRoad == yes, msg);
        }
    }

    internal class LongestRoadTest
    {
        IGameController Controller { get; set; }
        public LongestRoadTest(IGameController controller)
        {
            Controller = controller;
        }

        public async Task TestLongestRoad()
        {
            //
            //  all of these arrays are in reverse order because we use them to initialize a stack and Pop() takes off from the end of the array
            var player0Buildings = new int[] {16, 75, 3};
            var player1Buildings = new int[] {29, 65, 5};
            var player2Buildings = new int[] {77, 8};
            var player3Buildings = new int[] {22, 11};
            var player4Buildings = new int[] {50, 37};

            var player0Roads = new int[] {91, 92, 77, 76, 58, 59, 39, 21, 38, 22, 23, 6, 102, 3};
            var player1Roads = new int[] {89, 74, 73, 55, 56, 36, 34, 18, 19, 1, 88, 0};
            var player2Roads = new int[] {105, 8};
            var player3Roads = new int[] {28, 13};
            var player4Roads = new int[] {67, 47};


            var roadAndBuildings = new List<(Stack<int> buildings, Stack<int> roads)>() {
                (new Stack<int>(player0Buildings), new Stack<int>(player0Roads)),
                (new Stack<int>(player1Buildings), new Stack<int>(player1Roads)),
                (new Stack<int>(player2Buildings), new Stack<int>(player2Roads)),
                (new Stack<int>(player3Buildings), new Stack<int>(player3Roads)),
                (new Stack<int>(player4Buildings), new Stack<int>(player4Roads)),
            };

            await Controller.StartExpansionTestGame(assignResources: false, useCitiesAndKnights: true, playerCount: 5); // this test uses Knights

            var testHelper = new TestHelper(Controller);
            await testHelper.StartGame(roadAndBuildings);

       
            bool success;
            int playerIndex;
            int roadIndex;
    
            foreach (var player in Controller.PlayingPlayers)
            {
                player.Expect().LongestRoad(1);
                player.Expect().RoadCount(2);

            }

            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForRoll);
            await Controller.Test_DoRoll(3, 5, SpecialDice.Science);
            playerIndex = Controller.PlayingPlayers.IndexOf(Controller.CurrentPlayer);
            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForNext);
            Debug.Assert(playerIndex == 0);
            //
            // build 4 roads for player 0.  Should have 6 roads total, with a Longest Road of 5
            // also get 2 points.
            for (int i = 0; i < 4; i++)
            {
                Debug.Assert(roadAndBuildings[playerIndex].roads.Count > 0);
                roadIndex = roadAndBuildings[playerIndex].roads.Pop();
                await Controller.PurchaseAndPlaceRoad(roadIndex);
                if (i < 3)
                {
                    Expectations.Check(Controller, playerIndex: 0, roads: 3 + i, longestRoad: 2 + i, hasLongestRoad: false, score: 3);
                }


            }
            Expectations.Check(Controller, playerIndex: 0, roads: 6, longestRoad: 5, hasLongestRoad: true, score: 5);
      
            success = await Controller.NextState(); // move to Supplemental - PlayerOne last player to roll
            Debug.Assert(success);
            Debug.Assert(Controller.CurrentPlayer == Controller.PlayingPlayers[1]);
            Debug.Assert(Controller.CurrentGameState == GameState.Supplemental);
            Debug.Assert(Controller.LastPlayerToRoll == Controller.PlayingPlayers[0]);

            playerIndex = Controller.PlayingPlayers.IndexOf(Controller.CurrentPlayer);
            Debug.Assert(playerIndex == 1);
            // build roads for player 1
            for (int i = 0; i < 4; i++)
            {
                Debug.Assert(roadAndBuildings[playerIndex].roads.Count > 0);
                roadIndex = roadAndBuildings[playerIndex].roads.Pop();
                await Controller.PurchaseAndPlaceRoad(roadIndex);
                Expectations.Check(Controller, playerIndex: playerIndex, roads: 3 + i, longestRoad: 2 + i, hasLongestRoad: false, score: 3);
            }
            Expectations.Check(Controller, playerIndex: 0, roads: 6, longestRoad: 5, hasLongestRoad: true, score: 5);
            Expectations.Check(Controller, playerIndex: 1, roads: 6, longestRoad: 5, hasLongestRoad: false, score: 3);


            // build 7th road for player 1
            roadIndex = roadAndBuildings[playerIndex].roads.Pop();
            await Controller.PurchaseAndPlaceRoad(roadIndex);
            Expectations.Check(Controller, playerIndex: 0, roads: 6, longestRoad: 5, hasLongestRoad: false, score: 3);
            Expectations.Check(Controller, playerIndex: 1, roads: 7, longestRoad: 6, hasLongestRoad: true, score: 5);


            // undo the road - should lose longest road
            await Controller.DoUndo();
            Expectations.Check(Controller, playerIndex: 0, roads: 6, longestRoad: 5, hasLongestRoad: true, score: 5);
            Expectations.Check(Controller, playerIndex: 1, roads: 6, longestRoad: 5, hasLongestRoad: false, score: 3);


            await Controller.DoRedo(); // get it back
            Expectations.Check(Controller, playerIndex: 0, roads: 6, longestRoad: 5, hasLongestRoad: false, score: 3);
            Expectations.Check(Controller, playerIndex: 1, roads: 7, longestRoad: 6, hasLongestRoad: true, score: 5);

            // move to player 0 - this will be when the second player (player[1]) finishes their turn

            await Controller.Test_MoveToPlayer(0, GameState.Supplemental);
            Debug.Assert(Controller.CurrentPlayer == Controller.PlayingPlayers[0]);
            Debug.Assert(Controller.CurrentGameState == GameState.Supplemental);
            Debug.Assert(Controller.LastPlayerToRoll == Controller.PlayingPlayers[1]);

          

            // build a road towards Player[1]'s road
            roadIndex = roadAndBuildings[0].roads.Pop();
            await Controller.PurchaseAndPlaceRoad(roadIndex);
            Expectations.Check(Controller, playerIndex: 0, roads: 7, longestRoad: 5, hasLongestRoad: false, score: 3);
            Expectations.Check(Controller, playerIndex: 1, roads: 7, longestRoad: 6, hasLongestRoad: true, score: 5);


            // build and activate a knight to break the longest road
            await Controller.PurchaseAndPlaceKnight(roadAndBuildings[0].buildings.Pop(), true, KnightRank.Basic);
            Expectations.Check(Controller, playerIndex: 0, roads: 7, longestRoad: 5, hasLongestRoad: true, score: 5);
            Expectations.Check(Controller, playerIndex: 1, roads:7, longestRoad: 3, hasLongestRoad: false, score: 3);

            await Controller.NextState(); // should move to Player[2] WaitingForRoll
            Debug.Assert(Controller.CurrentPlayer == Controller.PlayingPlayers[2]);
            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForRoll);
            Debug.Assert(Controller.LastPlayerToRoll == Controller.PlayingPlayers[1]);

            // move to player 1's Supplemental build (which is the next time they can build)
            await Controller.Test_MoveToPlayer(1, GameState.Supplemental);
            Debug.Assert(Controller.CurrentPlayer == Controller.PlayingPlayers[1]);
            Debug.Assert(Controller.CurrentGameState == GameState.Supplemental);
            Debug.Assert(Controller.LastPlayerToRoll == Controller.PlayingPlayers[2]);

            // build and activate a knight to break to displace player0's knight
            await Controller.PurchaseAndPlaceKnight(roadAndBuildings[1].buildings.Pop(), true, KnightRank.Basic);
            Expectations.Check(Controller, playerIndex: 0, roads: 7, longestRoad: 5, hasLongestRoad: true, score: 5);
            Expectations.Check(Controller, playerIndex: 1, roads: 7, longestRoad: 3, hasLongestRoad: false, score: 3);

            // you can't displace a knight until right after you roll
            await Controller.Test_MoveToPlayer(1, GameState.WaitingForNext);
            Debug.Assert(Controller.CurrentPlayer == Controller.PlayingPlayers[1]);
            Debug.Assert(Controller.CurrentGameState == GameState.WaitingForNext);
            Debug.Assert(Controller.LastPlayerToRoll == Controller.PlayingPlayers[1]);


           

        }


    }
}
