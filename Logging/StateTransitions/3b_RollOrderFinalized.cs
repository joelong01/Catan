using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Catan10
{
    public class RollOrderFinalizedLog : LogHeader, ILogController
    {
        #region Methods

        public List<PlayerModel> PlayersInOrder {get; set;} = null;

        public static Task PostLog(IGameController gameController, List<PlayerModel> players)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRollForOrder);

            RollOrderFinalizedLog logHeader = new RollOrderFinalizedLog()
            {
                CanUndo = false,
                Action = CatanAction.ChangedState,
                NewState = GameState.FinishedRollOrder,
                PlayersInOrder = players
            };

            return gameController.PostMessage(logHeader, Catan.Proxy.ActionType.Normal);
        }

        //
        //  on .Do, just check as these have already been set...ReDo is different
        public Task Do(IGameController gameController)
        {
            Contract.Assert(PlayersInOrder.Count == gameController.PlayingPlayers.Count);
            for (int i=0; i<PlayersInOrder.Count; i++)
            {
                Contract.Assert(PlayersInOrder[i].PlayerIdentifier == gameController.PlayingPlayers[i].PlayerIdentifier);
            }

            
            return Task.CompletedTask;
        }
        /// <summary>
        ///     this is where we set the correct order for the players, having ignored the rolls previously
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public async Task Replay (IGameController gameController)
        {
            var playingPlayers = gameController.MainPageModel.PlayingPlayers;

            for (int i = 0; i < PlayersInOrder.Count; i++)
            {
                if (playingPlayers.Contains(PlayersInOrder[i]) == false)
                {
                    await gameController.AddPlayer(PlayersInOrder[i].PlayerName);
                }
                playingPlayers[i] = PlayersInOrder[i];
            }
        }

        //
        //  when redoing, set the order
        public Task Redo(IGameController gameController)
        {
            Contract.Assert(PlayersInOrder.Count == gameController.PlayingPlayers.Count);
            for (int i = 0; i < PlayersInOrder.Count; i++)
            {
                gameController.PlayingPlayers[i] = PlayersInOrder[i];
            }
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        #endregion Methods
    }
}