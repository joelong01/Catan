﻿using System;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     set when one player has a Gold tile rolled.  transitions to Waiting for Next when *all gold tiles* have been traded in.
    /// </summary>
    public class TradeGoldToWaitingForNext : LogHeader, ILogController
    {
        public static async Task PostMessage(IGameController gameController)
        {
            TradeGoldToWaitingForNext logHeader = new TradeGoldToWaitingForNext()
            {
                NewState = GameState.WaitingForNext,
                CanUndo = false
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);

        }

        public async Task Do(IGameController gameController)
        {
             await Task.Delay(0);
        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public async Task Replay (IGameController gameController)
        {
             await Task.Delay(0);
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }
    }
}
