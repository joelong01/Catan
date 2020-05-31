using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    /// <summary>
    /// This file should contain the information necessary to deal with the UI state in MainPage
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Fields

        private readonly Stack<GameState> _stateStack = new Stack<GameState>();
        public static readonly DependencyProperty LastRollProperty = DependencyProperty.Register("LastRoll", typeof(int), typeof(MainPage), new PropertyMetadata(0));

        #endregion Fields

        #region Properties

        public int LastRoll
        {
            get => (int)GetValue(LastRollProperty);
            set => SetValue(LastRollProperty, value);
        }

        public List<int> Rolls { get; set; } = new List<int>();

        #endregion Properties

        #region Methods

        public static int[] RollCount(IEnumerable<int> stack)
        {
            int[] counts = new int[11];
            Array.Clear(counts, 0, 11);
            if (stack.Count() != 0)
            {
                foreach (int n in stack)
                {
                    counts[n - 2]++;
                }
            }
            return counts;
        }

        public static double[] RollPercents(IEnumerable<int> stack, int[] counts)
        {
            double[] percents = new double[11];
            Array.Clear(percents, 0, 11);
            if (stack.Count() != 0)
            {
                int total = 0;
                foreach (int n in counts)
                {
                    total += n;
                }

                for (int i = 0; i < 11; i++)
                {
                    percents[i] = counts[i] / (double)stack.Count();
                }
            }

            return percents;
        }

        public async Task AnimatePlayers(int numberofPositions, LogType logType = LogType.Normal)
        {
            int index = GetNextPlayerPosition(numberofPositions);

            await AnimateToPlayerIndex(index, logType);
        }

        public async Task AnimateToPlayerIndex(int to, LogType logType = LogType.Normal)
        {
            var currentRandomGoldTiles = _gameView.CurrentRandomGoldTiles;
            List<int> newRandomGoldTiles = null;

            int from = MainPageModel.PlayingPlayers.IndexOf(CurrentPlayer);
            _currentPlayerIndex = to;

            // this is the one spot where the CurrentPlayer is changed.  it should update all the bindings
            // the setter will update all the associated state changes that happen when the CurrentPlayer
            // changes

            CurrentPlayer = MainPageModel.PlayingPlayers[_currentPlayerIndex];

            //
            //  we need to log what is the current state

            //
            // when we change player we optionally set tiles to be randomly gold - iff we are moving forward (not undo)
            // we need to check to make sure that we haven't already picked random goal tiles for this particular role.  the scenario is
            // we hit Next and are waiting for a role (and have thus picked random gold tiles) and then hit undo for some reason so that the
            // previous player can finish their turn.  when we hit Next again, we want the same tiles to be chosen to be gold.
            if (logType != LogType.Undo && (CurrentGameState == GameState.WaitingForNext || CurrentGameState == GameState.WaitingForRoll))
            {
                int playerRoll = TotalRolls / MainPageModel.PlayingPlayers.Count;  // integer divide - drops remainder
                if (playerRoll == CurrentPlayer.GameData.GoldRolls.Count)
                {
                    newRandomGoldTiles = GetRandomGoldTiles();
                    CurrentPlayer.GameData.GoldRolls.Add(newRandomGoldTiles);
                }
                else
                {
                    Debug.Assert(CurrentPlayer.GameData.GoldRolls.Count > playerRoll);
                    //
                    //  we've already picked the tiles for this roll -- use them
                    newRandomGoldTiles = CurrentPlayer.GameData.GoldRolls[playerRoll];
                }
                // this.TraceMessage($"[Player={CurrentPlayer} [PlayerRole={playerRoll}] [OldGoldTiles={StaticHelpers.SerializeList<int>(currentRandomGoldTiles)}] [NewGoldTiles={StaticHelpers.SerializeList<int>(newRandomGoldTiles)}]");
                await SetRandomTileToGold(newRandomGoldTiles);
            }
            else // undoing
            {
            }

            if (MainPageModel.Log != null)
            {
                // await AddLogEntry(CurrentPlayer, MainPageModel.Log.Last().GameState, CatanAction.ChangedPlayer, true, logType, -1, new LogChangePlayer(from, to, GameState.Unknown, currentRandomGoldTiles, newRandomGoldTiles));
            }
        }

        //
        //   find the next position in the _playerViewDictionary (where "next" is defined in PLAY_ORDER) that is set
        public int GetNextPlayerPosition(int count)
        {
            int index = _currentPlayerIndex + count;

            if (index >= MainPageModel.PlayingPlayers.Count)
            {
                index = index - MainPageModel.PlayingPlayers.Count;
            }

            if (index < 0)
            {
                index = index + MainPageModel.PlayingPlayers.Count;
            }

            return index;
        }

        public async Task PlayerWon()
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        public async Task SetFirst(PlayerModel player, LogType logType = LogType.Normal)
        {
            int idx = MainPageModel.PlayingPlayers.IndexOf(player);
            if (idx != -1)
            {
                for (int i = 0; i < idx; i++)
                {
                    PlayerModel pd = MainPageModel.PlayingPlayers[0];
                    MainPageModel.PlayingPlayers.RemoveAt(0);
                    MainPageModel.PlayingPlayers.Add(pd);
                }

                if (MainPageModel.Log != null)
                {
                    //  await AddLogEntry(CurrentPlayer, MainPageModel.Log.Last().GameState, CatanAction.SetFirstPlayer, true, logType, -1, new LogSetFirstPlayer(idx));
                }
            }

            await AnimateToPlayerIndex(0, logType);
        }

        #endregion Methods
    }

    public class MenuTag
    {
        #region Constructors

        public MenuTag(PlayerModel p)
        {
            Player = p;
        }

        public MenuTag(int n)
        {
            Number = n;
        }

        public MenuTag(StorageFile f)
        {
            File = f;
        }

        public MenuTag(IList<MenuFlyoutItemBase> list)
        {
            PeerMenuItemList = list;
        }

        public MenuTag()
        {
        }

        #endregion Constructors

        #region Properties

        public StorageFile File { get; set; }
        public int Number { get; set; }
        public IList<MenuFlyoutItemBase> PeerMenuItemList { get; set; }

        public PlayerModel Player { get; set; }
        public bool SetKeyUpHandler { get; set; } = false;

        #endregion Properties
    }
}
