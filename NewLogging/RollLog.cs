using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Catan10
{
    public interface IRollLog
    {
        bool CanRedo { get; }

        Task DoRoll(List<RollModel> rolls, List<int> goldTiles);

        Task RedoRoll();

        Task UndoRoll();
        int LastRoll { get; }
        List<RollModel> NextRolls { get; }
    }
    public interface IRollStats
    {
       string TwoPercent      {get;}
       string ThreePercent    {get;}
       string FourPercent     {get;}
       string FivePercent     {get;}
       string SixPercent      {get;}
       string SevenPercent    {get;}
       string EightPercent    {get;}
       string NinePercent     {get;}
       string TenPercent      {get;}
       string ElevenPercent   {get;}
       string TwelvePercent   {get;}
    }

    public enum RollAction { Do, Undo, Redo}

    /// <summary>
    ///     This class is used to track data that is global to the game -- Rolls and GoldTiles in particular
    /// </summary>
    public class RollLog : INotifyPropertyChanged, IRollLog, IRollStats
    {
        #region properties

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

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion properties

        #region DynamicProperties

        private string[] DynamicPropertyNames = new string[] { "TotalRolls", "LastRoll" };

        public int LastRoll
        {
            get
            {
                if (Done.Count == 0) return 0;
                return Done.Peek().SelectedRoll;
            }
        }

        public int TotalRolls => Done.Count;

        #endregion DynamicProperties

        private RollLog()
        {
        }

        public RollLog(IGameController gameController)
        {
            GameController = gameController;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Stack<RollState> Done { get; set; } = new Stack<RollState>();
        private IGameController GameController { get; set; }
        private Stack<RollState> Undone { get; set; } = new Stack<RollState>();

        public bool CanRedo
        {
            get
            {
                return Undone.Count != 0;
            }
        }

        public List<RollModel> NextRolls 
        { 
            get
            {
                if (Undone.Count > 0)
                {
                    return Undone.Peek().Rolls;
                }

                return null;
            }
        }

        private int[] RollCount(IEnumerable<RollState> stack)
        {
            int[] counts = new int[11];
            Array.Clear(counts, 0, 11);
            if (Done.Count != 0)
            {
                foreach (var rollState in stack)
                {
                    counts[rollState.SelectedRoll - 2]++;
                }
            }
            return counts;
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
                percents[i] = counts[i] / (double)counts.Length;
            }

            return (counts, percents);
        }

        private void UpdatePlayerStats(RollAction action)
        {
            int roll = 0;
            if (Done.Count != 0)
            {
                roll = Done.Peek().SelectedRoll;
            }

            
            int toAdd = 1;
            if (action == RollAction.Undo) toAdd = -1;
            PlayerModel sentBy = GameController.CurrentPlayer;
            //
            //   now update stats for the roll

            if (roll == 7)
            {
                if (action == RollAction.Undo)
                {
                    sentBy.GameData.MovedBaronAfterRollingSeven = false;
                }
                else
                {
                    sentBy.GameData.MovedBaronAfterRollingSeven = false;
                }

                foreach (PlayerModel player in GameController.PlayingPlayers)
                {

                    player.GameData.NoResourceCount += toAdd;

                }

                this.TraceMessage("Here is where you do the Baron work");
                // await SetStateAsync(CurrentPlayer, GameState.MustMoveBaron, false);
            }
            else
            {

                sentBy.GameData.MovedBaronAfterRollingSeven = null;

                //
                //  get the resources for the roll
                foreach (var player in GameController.PlayingPlayers)
                {
                    (TradeResources Granted, TradeResources Baroned) = GameController.ResourcesForRoll(player, roll);

                    player.GameData.Resources.GrantResources(Granted);
                    if (action == RollAction.Undo)
                    {
                        Baroned = Baroned.GetNegated();
                        Granted = Baroned.GetNegated();
                    }

                    player.GameData.Resources.ResourcesLostToBaron += Baroned;
                    GameController.MainPageModel.GameResources += Granted;

                    if (Granted.Count == 0)
                    {
                        player.GameData.GoodRoll = false; // how to undo this??
                        player.GameData.NoResourceCount += toAdd;
                    }
                    else
                    {
                        player.GameData.NoResourceCount = 0; // ??
                        player.GameData.GoodRoll = true;
                        player.GameData.RollsWithResource += toAdd;

                    }
                }

            }
        }

        private void UpdateRollStats()
        {
            var (Count, Percent) = RollPercents();

            TwoPercent = string.Format($"{Count[0]} ({Percent[0] * 100:0.#}%)");
            ThreePercent = string.Format($"{Count[1]} ({Percent[1] * 100:0.#}%)");
            FourPercent = string.Format($"{Count[2]} ({Percent[2] * 100:0.#}%)");
            FivePercent = string.Format($"{Count[3]} ({Percent[3] * 100:0.#}%)");
            SixPercent = string.Format($"{Count[4]} ({Percent[4] * 100:0.#}%)");
            SevenPercent = string.Format($"{Count[5]} ({Percent[5] * 100:0.#}%)");
            EightPercent = string.Format($"{Count[6]} ({Percent[6] * 100:0.#}%)");
            NinePercent = string.Format($"{Count[7]} ({Percent[7] * 100:0.#}%)");
            TenPercent = string.Format($"{Count[8]} ({Percent[8] * 100:0.#}%)");
            ElevenPercent = string.Format($"{Count[9]} ({Percent[9] * 100:0.#}%)");
            TwelvePercent = string.Format($"{Count[10]} ({Percent[10] * 100:0.#}%)");
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
                GameController.SetHighlightedTiles(Done.Peek().SelectedRoll);
                await GameController.SetRandomTileToGold(Done.Peek().GoldTiles);
            }
            else
            {
                GameController.StopHighlightingTiles();
                await GameController.ResetRandomGoldTiles();
            }
        }
        public Task DoRoll(List<RollModel> rolls, List<int> goldTiles)
        {
            Contract.Assert(Undone.Count == 0, "You can't add a roll if you have rolls in the unused stack");
            RollModel selected = rolls.Find((roll) => roll.Selected == true);
            Contract.Assert(selected != null);
            if (goldTiles == null) goldTiles = GameController.NextRandomGoldTiles;
            var rollState = new RollState() { SelectedRoll = selected.Roll, Rolls = rolls, GoldTiles = goldTiles };
            Done.Push(rollState);
            UpdatePlayerStats(RollAction.Do);
            return UpdateUi(RollAction.Do);
        }

        public Task RedoRoll()
        {
            Contract.Assert(CanRedo, "please check this before calling me");
            var rollState = Undone.Pop();
            Contract.Assert(rollState.PlayerName == GameController.CurrentPlayer.PlayerName);

            Done.Push(rollState);
            
            UpdatePlayerStats(RollAction.Redo);
            return UpdateUi(RollAction.Redo);
        }

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
        public List<int> GoldTiles { get; set; }
        public string PlayerName { get; set; } = MainPage.Current.CurrentPlayer.PlayerName;
        public List<RollModel> Rolls { get; set; }
        public int SelectedRoll { get; set; }

        // I want to use this for debugging...you should never apply a roll to a different player
        public override string ToString()
        {
            return $"[Roll={SelectedRoll}]";
        }
    }
}