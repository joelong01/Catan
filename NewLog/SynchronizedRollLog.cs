using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{

    public class SynchronizedRoll : IComparable<SynchronizedRoll>
    {
        public string PlayerName { get; set; }
        public List<int> Rolls { get; set; }
        public int DiceOne { get; set; }   // for the latest roll, what was the value of the first die?
        public int DiceTwo { get; set; }
        public override string ToString()
        {
            string s = "";
            Rolls.ForEach(r => s += $"{r},");
            return $"{PlayerName}: {s} ";
        }

        public int CompareTo(SynchronizedRoll other)
        {

            if (this.PlayerName == other.PlayerName) return 0;

            int max = this.Rolls.Count;
            if (other.Rolls.Count > max) max = other.Rolls.Count;



            for (int i = 0; i < max; i++)
            {
                if (i < this.Rolls.Count && i < other.Rolls.Count)
                {
                    if (this.Rolls[i] == other.Rolls[i]) continue;   // tie

                    if (this.Rolls[i] < other.Rolls[i])
                    {
                        return 1; // b bigger
                    }
                    else
                    {
                        return -1; // b smaller
                    }
                }
            }

            if (this.Rolls.Count == other.Rolls.Count) return 0;  // tie for all rolls!
                                                                  //
                                                                  //   this means that there is a tie, but somebody has extra rolls -- call it a ties

            return 0;
        }


    }

    public class SyncronizedPlayerRolls
    {
        public List<SynchronizedRoll> Rolls { get; set; } = new List<SynchronizedRoll>();
        public void Sort()
        {
            Rolls.Sort();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public bool HasTies()
        {
            Rolls.Sort();
            for (int i = 0; i < Rolls.Count - 1; i++)
            {
                if (Rolls[i].CompareTo(Rolls[i + 1]) == 0) return true;
            }

            return false; // if there is only 1 item, there are no ties



        }

        public bool InTie(SynchronizedRoll roll)
        {
            Contract.Assert(roll != null);
            if (roll.Rolls.Count == 0) return true;

            foreach (var r in Rolls)
            {
                if (r.CompareTo(roll) == 0)
                {
                    if (roll.Rolls.Count <= r.Rolls.Count) // the same or missing a roll
                    {
                    
                        return true;
                    }

                }
            }

            return false;
        }
    }

    public class SynchronizedRollLog : LogHeader, ILogController
    {
        public SyncronizedPlayerRolls PlayerRolls { get; set; } = new SyncronizedPlayerRolls();

        public static async Task<SynchronizedRollLog> StartSyncronizedRoll(IGameController gameController)
        {
            SynchronizedRollLog log = new SynchronizedRollLog()
            {
                CanUndo = false, 
                Action = CatanAction.RollToSeeWhoGoesFirst,
                NewState = GameState.WaitingForRollForOrder
            };
            await gameController.SynchronizedRoll(log);
            return log;
        }
        public Task Do(IGameController gameController, LogHeader logHeader)
        {
            SynchronizedRollLog log = logHeader as SynchronizedRollLog;
            return gameController.SynchronizedRoll(log);
        }

        public Task Redo(IGameController gameController, LogHeader logHeader)
        {
            SynchronizedRollLog log = logHeader as SynchronizedRollLog;
            return gameController.SynchronizedRoll(log);
        }

        public Task Undo(IGameController gameController, LogHeader logHeader)
        {
            throw new NotImplementedException();
        }
    }
}
