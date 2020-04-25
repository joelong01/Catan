using Catan.Proxy;
using System.Collections.Generic;

namespace Catan10
{


    /// <summary>
    ///     This class has all the data associated with a starting a game
    ///     1. Tiles
    ///     2. Harbors
    ///     3. Game
    /// </summary>
    public class StartGameModel : LogHeader
    {
        public RandomBoardSettings RandomBoardSettings { get; set; } = new RandomBoardSettings();
        public List<string> PlayerNames { get; set; } = new List<string>();
        public int GameIndex { get; set; }

        public StartGameModel() { }

        internal bool IsEqual(ChangedPlayerModel cpm)
        {
            return true;
        }
    }

    public class StartGameController
    {
        /// <summary>
        ///     this just constructs the StartGameModel object needed for the game to know to start
        ///     this is always the first log entry
        /// </summary>
        public static StartGameModel StartGame(MainPage page, int gameIndex, RandomBoardSettings rbs, IEnumerable<PlayerModel> playingPlayers)
        {
            StartGameModel model = new StartGameModel
            {
                Page = page,
                Player = page.CurrentPlayer,
                PlayerIndex = page.CurrentPlayer.AllPlayerIndex,
                PlayerName = page.CurrentPlayer.PlayerName,
                OldState = GameState.WaitingForStart,
                NewState = GameState.AllocateResourceForward,
                Action = CatanAction.Started,

                GameIndex = gameIndex,
                RandomBoardSettings = rbs
            };

            foreach (var player in playingPlayers)
            {
                model.PlayerNames.Add(player.PlayerName);
            }

            return model;

        }

        //
        //  no undo, no redo
    }
}