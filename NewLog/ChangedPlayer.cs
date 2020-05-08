using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Catan10
{


    /// <summary>
    ///     This class has all the data associated with changing a player.
    /// </summary>
    public class ChangePlayerLog : LogHeader, ILogController
    {
        public int Move { get; set; } = -1;
        public List<int> OldRandomGoldTiles { get; set; } = new List<int>();
        public List<int> NewRandomGoldTiles { get; set; } = new List<int>();
        public List<int> HighlightedTiles { get; set; } = new List<int>();
       
        public ChangePlayerLog() { }
        
        

        private bool ListsEqual(List<int> l1, List<int> l2)
        {
            if (l1.Count != l2.Count)
                return false;

            for (int i = 0; i < l1.Count; i++)
            {
                if (l1[i] != l2[i])
                {
                    return false;
                }
            }

            return true;
        }
        public static async Task<ChangePlayerLog> ChangePlayer(IGameController gameController,  int numberofPositions, GameState newState)
        {
            
            ChangePlayerLog changePlayerLog = new ChangePlayerLog
            {

                PlayerName = gameController.CurrentPlayer.PlayerName, // the value before we change -- e.g. where we go when we Undo
                OldState = gameController.CurrentGameState,
                NewState = newState,
                Action = CatanAction.ChangePlayerAndSetState,                
                Move = numberofPositions,
                OldRandomGoldTiles = gameController.CurrentRandomGoldTiles,
                NewRandomGoldTiles = gameController.NextRandomGoldTiles,
                HighlightedTiles = gameController.HighlightedTiles
              
            };

            await gameController.ChangePlayer(changePlayerLog);
            return changePlayerLog;

        }
        /// <summary>
        ///     These are not static because we use them when Deserializing log records, so we already have an instance.
        /// </summary>
        /// <param name="gameController"></param>
        /// <param name="logHeader"></param>
        /// <returns></returns>

        public Task Undo(IGameController gameController, LogHeader logHeader)
        {
            ChangePlayerLog log = logHeader as ChangePlayerLog;
            return gameController.UndoChangePlayer(log);
        }

        public Task Redo(IGameController gameController, LogHeader logHeader)
        {
            ChangePlayerLog log = logHeader as ChangePlayerLog;
            return gameController.ChangePlayer(log);            

        }

        public Task Do(IGameController gameController, LogHeader logHeader)
        {
            return gameController.ChangePlayer(logHeader as ChangePlayerLog);
        }
    }




}