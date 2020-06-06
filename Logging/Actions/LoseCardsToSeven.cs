using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    /// <summary>
    ///     Sent
    /// </summary>
    public class LoseCardsToSeven : LogHeader, ILogController
    {
        public Task Do(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }
    }
}
