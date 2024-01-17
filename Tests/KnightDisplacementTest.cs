using System;
using System.Collections.Generic;
using System.Diagnostics;
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


        }
    }
}
