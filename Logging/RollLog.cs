using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Catan10
{
    public enum RollAction { Do, Undo, Redo }

    /// <summary>
    ///     This class is used to track data that is global to the game -- Rolls in particular
    /// </summary>
    public class RollLog : INotifyPropertyChanged, IRollLog, IRollStats
    {
        private Stack<RollState> Done { get; set; } = new Stack<RollState>();

        private IGameController GameController { get; set; }

        private Stack<RollState> Undone { get; set; } = new Stack<RollState>();

        private string _eightPercent = "";

        private string _elevenPercent = "";

        private string _fivePercent = "";

        private string _fourPercent = "";

        private string _ninePercent = "";

        private string _sevenPercent = "";

        private string _sixPercent = "";

        private string _tenPercent = "";

        private string _threePercent = "";

        private string _twelvePercent = "";

        private string _twoPercent = "";

        private string[] DynamicPropertyNames = new string[] { "TotalRolls", "LastRoll", "GetRollCount", "GetRollPercent" };

        private RollLog()
        {
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int[] RollCount(IEnumerable<RollState> stack)
        {
            int[] counts = new int[11]; // yes, there really are 11 different possible rollModel
            Array.Clear(counts, 0, 11);
            if (Done.Count != 0)
            {
                //
                //  go through each rollModel and fill in the array with the count of what was rolled
                foreach (var rollState in stack)
                {
                    //12/20/2023 - when an Undo happens, the top of the stack is going to have a 0 in the rollModel.
                    //             just skip it.
                    if (rollState.RollModel.Roll > 1 && rollState.RollModel.Roll < 13)
                    {
                        counts[rollState.RollModel.Roll - 2]++;
                    }
                }
            }
            return counts;
        }

        public int GetRollCount(int roll)
        {
            try
            {
                if (Done == null) return 0;

                if (Done.Count == 0)
                {
                    return 0;
                }
                return RollCount(Done)[roll - 2];
            }
            catch
            {
                return -1;
            }
        }
        public string GetRollPercent(int roll)
        {
            if (Done == null) return "0%";
            if (Done.Count == 0)
            {
                return "0%";
            }
            var (_, Percent) = RollPercents();
            string percentFormat = "{0:#0.#}%";
            return string.Format(percentFormat, Percent[roll - 2] * 100);

        }

        private (int[] Count, double[] Percent) RollPercents()
        {
            int[] counts = RollCount(Done);

            double[] percents = new double[11];
            Array.Clear(percents, 0, 11);

            int total = 0;
            foreach (int n in counts)
            {
                total += n;
            }

            for (int i = 0; i < 11; i++)
            {
                percents[i] = counts[i] / ( double )total;
            }

            return (counts, percents);
        }
        /// <summary>
        ///     called while handling the rollModel processing
        /// </summary>
        /// <param name="action"></param>

        private void UpdatePlayerStats(RollAction action)
        {

            RollModel rollModel;

            int toAddForNoResourceCount = 1;
            if (action == RollAction.Undo)
            {
                toAddForNoResourceCount = -1;
                //12/20/2023: when undoing, the actuall rollModel is on the undo stack
                rollModel = Undone.Peek().RollModel;
            }
            else
            {
                rollModel = Done.Peek().RollModel;
            }

            //
            //   now update stats for the rollModel
            if (rollModel.Roll == 7)
            {

                foreach (PlayerModel player in GameController.PlayingPlayers)
                {
                    player.GameData.NoResourceCount += toAddForNoResourceCount;
                }
                return;
            }

            //
            //  get the resources for the rollModel
            foreach (var player in GameController.PlayingPlayers)
            {
                (TradeResources Granted, TradeResources Baroned) = GameController.ResourcesForRoll(player, rollModel, action);

                //12/20/2023: we pushed the action into ResourcesForRoll to make this work always
                player.GameData.Resources.ResourcesLostToBaron += Baroned;
                player.GameData.Resources.GrantResources(Granted);

                if (Granted.Count == 0)
                {
                    player.GameData.GoodRoll = false; // how to undo this??
                    player.GameData.NoResourceCount += toAddForNoResourceCount;
                }
                else
                {
                    player.GameData.NoResourceCount = 0; // ??
                    player.GameData.GoodRoll = true;
                    player.GameData.RollsWithResource += toAddForNoResourceCount;
                }
            }

        }

        #region count properties
        private int _twoCount = 0;
        private int _threeCount = 0;
        private int _fourCount = 0;
        private int _fiveCount = 0;
        private int _sixCount = 0;
        private int _sevenCount = 0;
        private int _eightCount = 0;
        private int _nineCount = 0;
        private int _tenCount = 0;
        private int _elevenCount = 0;
        private int _twelveCount = 0;

        public int TwoCount
        {
            get
            {
                return _twoCount;
            }
            set
            {
                if (value != _twoCount)
                {
                    _twoCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int ThreeCount
        {
            get
            {
                return _threeCount;
            }
            set
            {
                if (value != _threeCount)
                {
                    _threeCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int FourCount
        {
            get
            {
                return _fourCount;
            }
            set
            {
                if (value != _fourCount)
                {
                    _fourCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int FiveCount
        {
            get
            {
                return _fiveCount;
            }
            set
            {
                if (value != _fiveCount)
                {
                    _fiveCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int SixCount
        {
            get
            {
                return _sixCount;
            }
            set
            {
                if (value != _sixCount)
                {
                    _sixCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int SevenCount
        {
            get
            {
                return _sevenCount;
            }
            set
            {
                if (value != _sevenCount)
                {
                    _sevenCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int EightCount
        {
            get
            {
                return _eightCount;
            }
            set
            {
                if (value != _eightCount)
                {
                    _eightCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int NineCount
        {
            get
            {
                return _nineCount;
            }
            set
            {
                if (value != _nineCount)
                {
                    _nineCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int TenCount
        {
            get
            {
                return _tenCount;
            }
            set
            {
                if (value != _tenCount)
                {
                    _tenCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int ElevenCount
        {
            get
            {
                return _elevenCount;
            }
            set
            {
                if (value != _elevenCount)
                {
                    _elevenCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int TwelveCount
        {
            get
            {
                return _twelveCount;
            }
            set
            {
                if (value != _twelveCount)
                {
                    _twelveCount = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        private void UpdateRollStats()
        {

            if (Done.Count == 1 && Done.Peek().RollModel.Roll == 0)
            {
                TwoPercent = "";
                ThreePercent = "";
                FourPercent = "";
                FivePercent = "";
                SixPercent = "";
                SevenPercent = "";
                EightPercent = "";
                NinePercent = "";
                TenPercent = "";
                ElevenPercent = "";
                TwelvePercent = "";
            }
            else if (Done.Count > 0)
            {

                var (RollCounts, Percent) = RollPercents();
                TwoCount = RollCounts[0];
                ThreeCount = RollCounts[1];
                FourCount = RollCounts[2];
                FiveCount = RollCounts[3];
                SixCount = RollCounts[4];
                SevenCount = RollCounts[5];
                EightCount = RollCounts[6];
                NineCount = RollCounts[7];
                TenCount = RollCounts[8];
                ElevenCount = RollCounts[9];
                TwelveCount = RollCounts[10];

                // Format percentages with a consistent width for counts
                string percentFormat = "({0:#0.#}%)";
                TwoPercent = string.Format(percentFormat, Percent[0] * 100);
                ThreePercent = string.Format(percentFormat, Percent[1] * 100);
                FourPercent = string.Format(percentFormat, Percent[2] * 100);
                FivePercent = string.Format(percentFormat, Percent[3] * 100);
                SixPercent = string.Format(percentFormat, Percent[4] * 100);
                SevenPercent = string.Format(percentFormat, Percent[5] * 100);
                EightPercent = string.Format(percentFormat, Percent[6] * 100);
                NinePercent = string.Format(percentFormat, Percent[7] * 100);
                TenPercent = string.Format(percentFormat, Percent[8] * 100);
                ElevenPercent = string.Format(percentFormat, Percent[9] * 100);

            }
            else // 12/20/2023 - if you undo the first rollModel, you don't want to see NaN, you want empty strings
            {

                TwoPercent = "";
                ThreePercent = "";
                FourPercent = "";
                FivePercent = "";
                SixPercent = "";
                SevenPercent = "";
                EightPercent = "";
                NinePercent = "";
                TenPercent = "";
                ElevenPercent = "";
                TwelvePercent = "";
            }
        }

        private async Task UpdateUi(RollAction action)
        {
            UpdateRollStats();

            //
            //  tell the Views that the dynamic (calcluated) properites have changed
            DynamicPropertyNames.ForEach((name) => NotifyPropertyChanged(name));

            // update the tiles -- this should be fixed to be done with data bindings
            //
            if (action != RollAction.Undo)
            {
                GameController.SetHighlightedTiles(Done.Peek().RollModel.Roll);
            }
            else
            {
                GameController.StopHighlightingTiles();
            }

            await Task.Delay(0);
        }

        internal Task UpdateUiForRoll(RollState rollState)
        {
            
            Done.Push(rollState);
            NotifyPropertyChanged("LastRoll");
            UpdatePlayerStats(RollAction.Do);
            return this.UpdateUi(RollAction.Do);
        }

        public bool CanRedo
        {
            get
            {
                return Undone.Count != 0;
            }
        }

        public string EightPercent
        {
            get
            {
                return _eightPercent;
            }
            set
            {
                if (_eightPercent != value)
                {
                    _eightPercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ElevenPercent
        {
            get
            {
                return _elevenPercent;
            }
            set
            {
                if (_elevenPercent != value)
                {
                    _elevenPercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string FivePercent
        {
            get
            {
                return _fivePercent;
            }
            set
            {
                if (_fivePercent != value)
                {
                    _fivePercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string FourPercent
        {
            get
            {
                return _fourPercent;
            }
            set
            {
                if (_fourPercent != value)
                {
                    _fourPercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RollModel LastRoll
        {
            get
            {
                if (Done == null) return null;
                if (Done.Count == 0) return null;
                return Done.Peek().RollModel;
            }
        }

        public RollModel NextRoll
        {
            get
            {
                if (Undone.Count > 0)
                {
                    return Undone.Peek().RollModel;
                }

                return null;
            }
        }

        public string NinePercent
        {
            get
            {
                return _ninePercent;
            }
            set
            {
                if (_ninePercent != value)
                {
                    _ninePercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string SevenPercent
        {
            get
            {
                return _sevenPercent;
            }
            set
            {
                if (_sevenPercent != value)
                {
                    _sevenPercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string SixPercent
        {
            get
            {
                return _sixPercent;
            }
            set
            {
                if (_sixPercent != value)
                {
                    _sixPercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string TenPercent
        {
            get
            {
                return _tenPercent;
            }
            set
            {
                if (_tenPercent != value)
                {
                    _tenPercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ThreePercent
        {
            get
            {
                return _threePercent;
            }
            set
            {
                if (_threePercent != value)
                {
                    _threePercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int TotalRolls => Done.Count;

        public string TwelvePercent
        {
            get
            {
                return _twelvePercent;
            }
            set
            {
                if (_twelvePercent != value)
                {
                    _twelvePercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string TwoPercent
        {
            get
            {
                return _twoPercent;
            }
            set
            {
                if (_twoPercent != value)
                {
                    _twoPercent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RollLog(IGameController gameController)
        {
            GameController = gameController;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Task DoRoll(RollModel rollModel)
        {
            Contract.Assert(Undone.Count == 0, "You can't add a roll if you have roll in the unused stack");

            var rollState = new RollState()
            {
               
                RollModel = rollModel, 
            };
            Done.Push(rollState);
            UpdatePlayerStats(RollAction.Do);
            return UpdateUi(RollAction.Do);
        }

        /// <summary>
        ///     before you rollModel, you get the Undone RollModel
        /// </summary>
        /// <returns></returns>
        public RollState PopUndoneRoll()
        {
            if (Undone.Count == 0) return null;
            return Undone.Pop();
        }

        /// <summary>
        ///     We have a RollState on the Undone stack that has the rollModel and we have a RollState on the Done stack
        ///     that has a 0 for the rollModel. we pop the Undone and throw it away after setting the rollModel in the Done stack
        /// </summary>
        /// <returns></returns>
        public Task RedoRoll()
        {
            Contract.Assert(CanRedo, "please check this before calling me");
            var undoneRollState = Undone.Pop();
            Done.Push(undoneRollState);

            UpdatePlayerStats(RollAction.Redo);
            return UpdateUi(RollAction.Redo);
        }

        /// <summary>
        ///     after a rollModel is completed, go to the RollStack and stick it into the top rollModel
        /// </summary>
        /// <param name="rolls"></param>
        /// <returns></returns>
        public Task SetLastRoll(RollModel rollModel)
        {
            RollState rollState = Done.Peek();

            Debug.Assert(rollModel.Roll > 1 && rollModel.Roll < 13);
            rollState.RollModel = rollModel;
            UpdatePlayerStats(RollAction.Do);
            return UpdateUi(RollAction.Do);
        }

        /// <summary>
        ///     12/20/2023 - unfortunately when the idiot dev implemented this, there are 2 actions that are done in one log entry
        ///     the first is setting the random gold tiles, the second is setting the rollModel. there was a bug where both got "undone"
        ///     so here, we create a new undoneRollState that has the correct random gold tiles, but the rollModel set to 0 and we leave that 
        ///     
        ///     12/28/2023 - the idiot dev pulled all reference to gold tiles from this class.  thank god.
        ///     in the Done stack.
        /// </summary>
        /// <returns></returns>

        public Task UndoRoll()
        {
            Contract.Assert(Done.Count != 0, "please check this before calling me");

            RollState rollState = Done.Pop(); 
            Undone.Push(rollState);
            UpdatePlayerStats(RollAction.Undo);
            return UpdateUi(RollAction.Undo);
        }
    }

    /// <summary>
    ///     Nothing binds directly to this data, so it doesn't implement INotifyPropertyChanged
    /// </summary>
    public class RollState
    {
        public string PlayerName { get; set; } = MainPage.Current.CurrentPlayer.PlayerName;
        public RollModel RollModel { get; set; }

        // I want to use this for debugging...you should never apply a rollModel to a different player
        public override string ToString()
        {
            return $"[Roll={RollModel}]";
        }
    }

    public interface IRollLog
    {
        bool CanRedo { get; }

        RollModel LastRoll { get; }

        RollModel NextRoll { get; }

        Task DoRoll(RollModel rollModel);

        Task RedoRoll();

        Task UndoRoll();
    }

    public interface IRollStats
    {
        string EightPercent { get; }
        string ElevenPercent { get; }
        string FivePercent { get; }
        string FourPercent { get; }
        string NinePercent { get; }
        string SevenPercent { get; }
        string SixPercent { get; }
        string TenPercent { get; }
        string ThreePercent { get; }
        string TwelvePercent { get; }
        string TwoPercent { get; }
    }
}