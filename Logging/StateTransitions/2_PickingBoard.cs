using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Get a random board and send it to all the players.
    ///     Turn on notifications for game data
    /// </summary>
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
                NewState = GameState.PickingBoard,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            var mainPageModel = gameController.MainPageModel;
            mainPageModel.FinishedAddingPlayers();

            if (mainPageModel.GameInfo.Creator == gameController.TheHuman.PlayerName || mainPageModel.Settings.IsLocalGame)
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

        public async Task Replay (IGameController gameController)
        {
            gameController.MainPageModel.FinishedAddingPlayers();
            await Task.Delay(0);
        }

        public async Task Redo(IGameController gameController)
        {
            //
            //  do any state cleanup needed for transitioning into Pick board
            gameController.ShowRollsInPublicUi();
            if (gameController.MainPageModel.GameInfo.Creator == gameController.TheHuman.PlayerName)
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

        public async Task Undo(IGameController gameController)
        {
            await Task.Delay(0);
        }
    }
}