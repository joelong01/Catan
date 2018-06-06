using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        Dictionary<PlayerPosition, PlayerView> _playerViewDictionary = new Dictionary<PlayerPosition, PlayerView>(); // this has all the players and their location of the current game        
        List<int> _rolls = new List<int>(); // a useful cache of all the rolls the players have made
        Stack<GameState> _stateStack = new Stack<GameState>();
        

        //
        //  the problem is that the GameTracker works off a list and used to not care about the physical position of the players.
        //  the rule was whoever was "on top" in the UI went.  Now we care about the physical position of the board and have to translate 
        //  so that the List we pass to the game tracker is in the right order.  we should fix this...
        private PlayerPosition[] PLAY_ORDER = new PlayerPosition[] { PlayerPosition.BottomLeft, PlayerPosition.Left, PlayerPosition.TopLeft, PlayerPosition.TopRight, PlayerPosition.Right, PlayerPosition.BottomRight };

        //
        //  move to player and show message.  wait for Enter and return what was typed in
        public async Task<string> ShowAndWait(string message, string value, bool enableInput = true)
        {


            _btnNextStep.Content = message;
            StateDescription = value;
            _btnNextStep.IsEnabled = true;
            await _btnNextStep.WhenClicked();
            return "";


        }

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
                return false;

            _rolls.Push(roll);
            CurrentPlayerRolls.Push(roll);
            //   this.TraceMessage($"\n{CurrentPlayer.PlayerName} rolled {roll}\n");
            return true;
        }



        public int PopRoll()
        {
            if (_rolls.Count == 0)
                return -1;

            int n = _rolls.Pop();
            int m = CurrentPlayerRolls.Pop();

            return n;
        }

        private PlayerData CurrentPlayer
        {
            get
            {
                if (PlayingPlayers == null) return null;
                if (PlayingPlayers.Count == 0) return null;

                return PlayingPlayers[_currentPlayerIndex];                
            }           
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

        public async Task AnimateToPlayer(PlayerData player, LogType logType = LogType.Normal)
        {
            int idx = PlayingPlayers.IndexOf(player);
            if (idx != -1)
            {
                await AnimateToPlayerIndex(idx, logType);
            }
        }

        public async Task SetFirst(PlayerData player, LogType logType = LogType.Normal)
        {
            int idx = PlayingPlayers.IndexOf(player);
            if (idx != -1)
            {
                for (int i=0; i<idx; i++)
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
            if(_menuHidePlayersOnNext.IsChecked)
            {
                foreach (var player in PlayingPlayers)
                {
                   // TODO: do you want this functionality?  if so, you need to get an event fired that 
                   //       goes off when the CurrentPlayer changes so that the view can show/hide itself
                    // player.Close();
                }
            }

            
            int from = PlayingPlayers.IndexOf(CurrentPlayer);            
            _currentPlayerIndex = to;
            //To.Open();
            CurrentPlayerChanged();
            if (_log != null)
            {
                await AddLogEntry(CurrentPlayer, _log.Last().GameState, CatanAction.ChangedPlayer, true, logType, -1, new LogChangePlayer(from, to));
            }

        }

       

        public async Task AnimatePlayers(int numberofPositions, LogType logType = LogType.Normal)
        {
            int index = GetNextPlayerPosition(numberofPositions);

           await  AnimateToPlayerIndex(index, logType);
        }

        public async Task AnimateToPreviousPlayer(LogType logType = LogType.Normal)
        {
            await AnimatePlayers(-1, logType);
        }
        public async Task AnimateToNextPlayer(LogType logType = LogType.Normal)
        {
            await AnimatePlayers(1, logType);
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
                    percents[i] = (double)counts[i] / (double)stack.Count();
                }

            }

            return percents;
        }

        public ObservableCollection<int> CurrentPlayerRolls
        {
            get

            {
                return CurrentPlayer.GameData.Rolls;
            }
        }

        public List<int> AllRolls
        {
            get
            {
                return _rolls;
            }
            set
            {
                _rolls = value;
            }
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

    public static class MenuExtensions
    {
        public static MenuTag MenuTag(this MenuFlyoutSubItem menu)
        {
            return menu.Tag as MenuTag;
        }

        public static MenuTag MenuTag(this MenuFlyoutItem menu)
        {
            return menu.Tag as MenuTag;
        }

        public static MenuTag MenuTag(this MenuFlyoutItemBase menu)
        {
            return menu.Tag as MenuTag;
        }



        public static MenuTag AddOrCreateMenuTag(this MenuFlyoutItemBase menu, PlayerData p)
        {
            MenuTag tag = menu.MenuTag();
            if (tag == null)
            {
                tag = new MenuTag();
                menu.Tag = tag;
            }
            tag.Player = p;
            return tag;


        }

        public static MenuTag AddOrCreateMenuTag(this MenuFlyoutItemBase menu, StorageFile f)
        {
            MenuTag tag = menu.MenuTag();
            if (tag == null)
            {
                tag = new MenuTag();
                menu.Tag = tag;
            }
            tag.File = f;
            return tag;
        }
        public static MenuTag AddOrCreateMenuTag(this MenuFlyoutItemBase menu, IList<MenuFlyoutItemBase> list)
        {
            MenuTag tag = menu.MenuTag();
            if (tag == null)
            {
                tag = new MenuTag();
                menu.Tag = tag;
            }
            tag.PeerMenuItemList = list;
            return tag;
        }

        public static MenuTag AddOrCreateMenuTag(this MenuFlyoutItemBase menu, int n)
        {
            MenuTag tag = menu.MenuTag();
            if (tag == null)
            {
                tag = new MenuTag();
                menu.Tag = tag;
            }
            tag.Number = n;
            return tag;
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