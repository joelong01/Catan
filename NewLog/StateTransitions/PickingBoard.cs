﻿using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;
using Catan10;

namespace Catan10
{
    public class WaitingForPlayersToPickingBoard : LogHeader, ILogController
    {
        public WaitingForPlayersToPickingBoard() : base()
        {
        }

        public static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForPlayers);

            WaitingForPlayersToPickingBoard logHeader = new WaitingForPlayersToPickingBoard()
            {
                CanUndo = false,
                Action = CatanAction.ChangedState,
                OldState = GameState.WaitingForPlayers,
                NewState = GameState.PickingBoard,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            var mainPageModel = gameController.MainPageModel;
            if (mainPageModel.GameStartedBy == gameController.TheHuman)
            {
                //
                //  we only need one person sending around a random board
                await RandomBoardLog.RandomizeBoard(gameController, 0);
                if (mainPageModel.Settings.AutoRespond)
                {
                    //
                    //  simulate clicking on Next
                    await PickingBoardToWaitingForRollOrder.PostLog(gameController);
                }
            }
            
        }

        public  async Task Redo(IGameController gameController)
        {
            //
            //  do any state cleanup needed for transitioning into Pick board
            await gameController.ResetRollControl();
            if (gameController.MainPageModel.GameStartedBy == gameController.TheHuman)
            {
                //
                //  The person that starts the game controlls the board
                await RandomBoardLog.RandomizeBoard(gameController, 0);

                if (gameController.AutoRespondAndTheHuman)
                {
                    //
                    //  simulate clicking on Next
                    await PickingBoardToWaitingForRollOrder.PostLog(gameController);
                }
               
            }
        }

        public override string ToString()
        {
            return $"[Action={Action}][SentBy={SentBy}][OldState={OldState}][NewState={NewState}]";
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }
    }
}