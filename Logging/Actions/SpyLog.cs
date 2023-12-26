using System;
using System.Threading.Tasks;

namespace Catan10
{
    public class CatanSpyLog : LogHeader, ILogController
    {
        public bool SpyOn { get; set; }

        public static Task SpyOnOff(IGameController gameController, bool spyOn)
        {
            CatanSpyLog logHeader = new CatanSpyLog()
            {
                SpyOn = spyOn,
                CanUndo = false
            };
            return gameController.PostMessage(logHeader, ActionType.Normal);
        }
        #region Methods

        public Task Do(IGameController gameController)
        {
            gameController.SetSpyInfo(this.SentBy.PlayerName, this.SpyOn);
            return Task.CompletedTask;
        }
        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}