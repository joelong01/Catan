using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Verify
    ///     1. each player in the right order
    ///     2. CurrentPlayer
    /// </summary>
    public class VerifyPlayers : LogHeader, ILogController
    {
        public PlayerModel CurrentPlayer { get; set; }
        public List<PlayerModel> PlayingPlayers { get; set; }

        public static async Task PostMessage(IGameController gameController)
        {
            var log = new VerifyPlayers()
            {
                Action = CatanAction.Verify,
                LogType = LogType.DoNotLog,
                PlayingPlayers = gameController.PlayingPlayers,
                CurrentPlayer = gameController.CurrentPlayer
            };

            await gameController.PostMessage(log, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            Contract.Assert(this.PlayingPlayers.Count == gameController.PlayingPlayers.Count);
            for (int i = 0; i < PlayingPlayers.Count; i++)
            {
                Debug.Assert(this.PlayingPlayers[i].PlayerName == gameController.PlayingPlayers[i].PlayerName);
            }
            Contract.Assert(this.CurrentPlayer.PlayerName == gameController.CurrentPlayer.PlayerName);
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }
    }
}