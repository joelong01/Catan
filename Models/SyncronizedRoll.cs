using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Catan10
{
    /// <summary>
    ///     this class has
    ///         1. the list of the rolls a player has made
    ///         2. the values of the 2 dice rolled
    ///     It also knows
    ///         1. How to compare the full list of Rolls while preserving order
    ///            (e.g. person 1 with rolls 5,7,7,4 wins over person 2 with 5,7,7,3 and person 3 with rolls 6 wins over all)
    ///         2. how to tell if to SynchronizedPlayerRolls are in a tie
    /// </summary>

    public class SyncronizedPlayerRolls : IComparable<SyncronizedPlayerRolls>, INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private RollModel _rollModel = new RollModel();

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        private bool _mustRoll = true;

        public RollModel CurrentRoll
        {
            get
            {
                return _rollModel;
            }
            set
            {
                if (_rollModel != value)
                {
                    _rollModel = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("LatestRoll");
                    NotifyPropertyChanged("ShowLatestRoll");
                }
            }
        }
        public ObservableCollection<RollModel> LatestRolls { get; set; } = new ObservableCollection<RollModel>();

        public bool MustRoll
        {
            get
            {
                return _mustRoll;
            }
            set
            {
                if (_mustRoll != value)
                {
                    _mustRoll = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public List<int> RollValues { get; set; } = new List<int>();

        public bool ShowLatestRoll
        {
            get
            {
                return ((CurrentRoll.DiceOne > 0 && CurrentRoll.DiceTwo > 0));
            }
        }

        #endregion Properties

        #region Constructors + Destructors

        public SyncronizedPlayerRolls()
        {
            LatestRolls.AddRange(PickingBoardToWaitingForRollOrder.GetRollModelList()); // always have 4 Rolls in here so XAML doesn't throw
        }

        #endregion Constructors + Destructors

        #region Methods

        public RollModel AddRoll(List<RollModel> rolls)
        {
            //
            //  5/26/2020:  Don't clear and add range -- XAML will barf because i'm accessing the indexers in PublicDataCtrl.xaml

            Contract.Assert(rolls.Count == 4);
            for (int i = 0; i < rolls.Count; i++)
            {
                LatestRolls[i].Selected = rolls[i].Selected;
                LatestRolls[i].DiceOne = rolls[i].DiceOne;
                LatestRolls[i].DiceTwo = rolls[i].DiceTwo;
                LatestRolls[i].Orientation = rolls[i].Orientation;
                if (rolls[i].Selected)
                {
                    CurrentRoll = LatestRolls[i];
                }
            }
            NotifyPropertyChanged("LatestRolls");
            MustRoll = false;
            RollValues.Add(CurrentRoll.DiceOne + CurrentRoll.DiceTwo);
            return CurrentRoll;
        }

        public int CompareTo(SyncronizedPlayerRolls other)
        {
            if (this.RollValues.Count == 0)
            {
                return this.RollValues.Count - other.RollValues.Count;
            }
            if (other.RollValues.Count == 0)
            {
                return -1;
            }

            int max = Math.Max(this.RollValues.Count, other.RollValues.Count);

            for (int i = 0; i < max; i++)
            {
                if (i < this.RollValues.Count && i < other.RollValues.Count)
                {
                    if (this.RollValues[i] == other.RollValues[i]) continue;   // tie

                    if (this.RollValues[i] < other.RollValues[i])
                    {
                        return 1; // b bigger
                    }
                    else
                    {
                        return -1; // b smaller
                    }
                }
            }

            if (this.RollValues.Count == other.RollValues.Count) return 0;  // tie for all rolls!
                                                                            //
                                                                            //   this means that there is a tie, but somebody has extra rolls -- call it a ties

            return 0;
        }

        public bool InTie(SyncronizedPlayerRolls roll)
        {
            Contract.Assert(roll != null);
            if (roll.RollValues.Count == 0) return true;

            foreach (var r in RollValues)
            {
                if (r.CompareTo(roll) == 0)
                {
                    if (roll.RollValues.Count <= this.RollValues.Count) // the same or missing a roll
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TiedWith(List<int> rolls)
        {
            if (Math.Abs(rolls.Count - RollValues.Count) > 1) return false; //

            int count = Math.Min(rolls.Count, RollValues.Count);
            for (int i = 0; i < count; i++)
            {
                if (rolls[i] != RollValues[i])
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        // a list of the chosen roll values -- used to break ties
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}