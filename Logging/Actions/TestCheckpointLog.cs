using System.Threading.Tasks;

namespace Catan10
{
    public class TestCheckpointLog : LogHeader, ILogController
    {
        private enum CheckPointAction { AddTest, FinishTest }
        GameState StartState { get; set; }
        CheckPointAction LogAction { get; set; }
        public static async Task AddTestCheckpoint(IGameController controller)
        {

            var logEntry = new TestCheckpointLog()
            {
                StartState = controller.CurrentGameState,
                NewState  = GameState.TestCheckpoint,
                LogAction = CheckPointAction.AddTest,
                UndoNext = false
            };

            await controller.PostMessage(logEntry, ActionType.Normal);
        }
        private static async Task BackToPreviousState(IGameController controller, GameState state)
        {
            var logEntry = new TestCheckpointLog()
            {
                NewState = state,
                UndoNext = false,
                LogAction = CheckPointAction.FinishTest
            };
            await controller.PostMessage(logEntry, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            if (this.LogAction == CheckPointAction.AddTest)
            {
                await TestCheckpointLog.BackToPreviousState(gameController, StartState);
            }
            await DefaultTask;
        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Replay(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            await DefaultTask;
        }
    }
}
