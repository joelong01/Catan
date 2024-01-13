using System.Diagnostics;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///
    /// the general flow is
    ///
    ///     WaitingForRoll => Rolled 7 => MustMoveBaronLog => MoveBaronLog => WaitingForNext
    ///                             or
    ///     WaitingForRoll => PlayedKnight => MustMoveBaronLog => MoveBaronLog => WaitingForRoll
    ///                             or
    ///     WaitingForRoll => WaitingForNext => PlayedKnight => MustMoveBaronLog => MoveBaronLog => WaitingForRoll
    ///
    /// </summary>
    public class MustMoveBaronLog : LogHeader, ILogController
    {
        #region Properties

        public MoveBaronReason Reason { get; set; }
        public GameState StartingState { get; set; }

        #endregion Properties

        #region Methods

        public static async Task PostLog (IGameController gameController, MoveBaronReason reason)
        {
            if (gameController.CurrentGameState != GameState.WaitingForNext &&
                            gameController.CurrentGameState != GameState.WaitingForRoll)
            {
                gameController.TraceMessage($"strange -- investigate.  currentstate is {gameController.CurrentGameState}");
            }

            if (reason == MoveBaronReason.Rolled7)
            {
                Debug.Assert(gameController.CurrentGameState == GameState.TooManyCards || gameController.CurrentGameState == GameState.WaitingForNext);
            }

            MustMoveBaronLog logHeader = new MustMoveBaronLog()
            {
                NewState = GameState.MustMoveBaron,
                CanUndo = true,
                Reason = reason,
                StartingState = gameController.CurrentGameState,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do (IGameController gameController)
        {
            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.MoveBaron);

             await Task.Delay(0);
        }

        public async Task Redo (IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Replay (IGameController gameController)
        {
       
            await Task.Delay(0);
        }

        public async Task Undo (IGameController gameController)
        {
            gameController.CurrentPlayer.GameData.Resources.RevokeEntitlement(Entitlement.MoveBaron);
            await Task.Delay(0);
        }

        #endregion Methods
    }
}