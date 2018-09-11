﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{


    /// <summary>
    /// This file should contain the information necessary to deal with the UI state in MainPage
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public List<int> Rolls { get; set; } = new List<int>();
        Stack<GameState> _stateStack = new Stack<GameState>();


        //
        //  the problem is that the GameTracker works off a list and used to not care about the physical position of the players.
        //  the rule was whoever was "on top" in the UI went.  Now we care about the physical position of the board and have to translate 
        //  so that the List we pass to the game tracker is in the right order.  we should fix this...
        private PlayerPosition[] PLAY_ORDER = new PlayerPosition[] { PlayerPosition.BottomLeft, PlayerPosition.Left, PlayerPosition.TopLeft, PlayerPosition.TopRight, PlayerPosition.Right, PlayerPosition.BottomRight };



        public async Task PlayerWon()
        {
            await Task.Delay(0);
            throw new NotImplementedException();
#if false
            foreach (CatanPlayer p in _players)
            {
                int gp = Int32.Parse(p.GamesPlayed);
                gp++;
                p.GamesPlayed = gp.ToString();

            }

            int gw = Int32.Parse(_players[_currentPlayerIndex].GamesWon);
            gw++;
            _players[_currentPlayerIndex].GamesWon = gw.ToString();


            //
            //  read in all the players because some might not have played
            ObservableCollection<CatanPlayer> allPlayers = await MainPage.LoadPlayers(MainPage.PlayerDataFile);

            IEnumerable<CatanPlayer> unionPlayers = MergeInto<CatanPlayer>(_players, allPlayers, new CatanPlayerComparer());

            await MainPage.SavePlayers(unionPlayers, MainPage.PlayerDataFile);
#endif

        }
        public bool PushRoll(int roll)
        {

            if (roll < 2 || roll > 12)
            {
                return false;
            }

            Rolls.Push(roll);

            PostLogEntry(CurrentPlayer, GameState.WaitingForRoll, CatanAction.Rolled, true, LogType.Normal, roll);
            UpdateRollStats();
            return true;
        }



        public int PopRoll()
        {
            if (Rolls.Count == 0)
            {
                return -1;
            }

            int lastRoll = Rolls.Pop();
            PostLogEntry(CurrentPlayer, GameState, CatanAction.Rolled, true, LogType.Undo, lastRoll);
            UpdateRollStats();
            return lastRoll;
        }




        //
        //   find the next position in the _playerViewDictionary (where "next" is defined in PLAY_ORDER) that is set
        int GetNextPlayerPosition(int count)
        {

            int index = _currentPlayerIndex + count;

            if (index >= PlayingPlayers.Count)
            {
                index = index - PlayingPlayers.Count;
            }

            if (index < 0)
            {
                index = index + PlayingPlayers.Count;
            }

            return index;
        }

        public async Task SetFirst(PlayerData player, LogType logType = LogType.Normal)
        {
            int idx = PlayingPlayers.IndexOf(player);
            if (idx != -1)
            {
                for (int i = 0; i < idx; i++)
                {
                    PlayerData pd = PlayingPlayers[0];
                    PlayingPlayers.RemoveAt(0);
                    PlayingPlayers.Add(pd);
                }

                if (_log != null)
                {
                    await AddLogEntry(CurrentPlayer, _log.Last().GameState, CatanAction.SetFirstPlayer, true, logType, -1, new LogSetFirstPlayer(idx));
                }
            }

            await AnimateToPlayerIndex(0, logType);
        }

        public async Task AnimateToPlayerIndex(int to, LogType logType = LogType.Normal)
        {
            if (_menuHidePlayersOnNext.IsChecked)
            {
                foreach (PlayerData player in PlayingPlayers)
                {
                    // TODO: do you want this functionality?  if so, you need to get an event fired that 
                    //       goes off when the CurrentPlayer changes so that the view can show/hide itself
                    // player.Close();
                }
            }


            int from = PlayingPlayers.IndexOf(CurrentPlayer);
            _currentPlayerIndex = to;

            // this is the one spot where the CurrentPlayer is changed.  it shoudl update all the bindings
            // the setter will update all the associated state changes that happen when the CurrentPlayer
            // changes

            CurrentPlayer = PlayingPlayers[_currentPlayerIndex];


            if (_log != null)
            {
                await AddLogEntry(CurrentPlayer, _log.Last().GameState, CatanAction.ChangedPlayer, true, logType, -1, new LogChangePlayer(from, to));
            }

        }



        public async Task AnimatePlayers(int numberofPositions, LogType logType = LogType.Normal)
        {
            int index = GetNextPlayerPosition(numberofPositions);

            await AnimateToPlayerIndex(index, logType);
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
    }

    public class MenuTag
    {
        public int Number { get; set; }
        public StorageFile File { get; set; }
        public IList<MenuFlyoutItemBase> PeerMenuItemList { get; set; }

        public PlayerData Player { get; set; }
        public bool SetKeyUpHandler { get; set; } = false;
        public MenuTag(PlayerData p)
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
        public MenuTag() { }
    }


}