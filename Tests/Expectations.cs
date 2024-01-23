using System.Diagnostics;

namespace Catan10
{

    public class Expectations
    {
        public static void Check(IGameController controller, int playerIndex, int roads, int longestRoad, bool hasLongestRoad, int score)
        {
            var player = controller.PlayingPlayers[playerIndex];
            Debug.Assert(roads == player.GameData.Roads.Count);
            Debug.Assert(longestRoad == player.GameData.LongestRoad);
            Debug.Assert(hasLongestRoad == player.GameData.HasLongestRoad);
            Debug.Assert(score == player.GameData.Score);
            Debug.Assert(player.CalculateLongestRoad() == longestRoad);
        }
    }

}

