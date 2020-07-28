using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Catan10
{
    /// <summary>
    ///     Sent
    /// </summary>
    public class LoseCardsToSeven : LogHeader, ILogController
    {
        public TradeResources LostResources { get; set; }
        public static Task PostMessage(IGameController gameController, TradeResources lostCards)
        {
            Contract.Assert(lostCards.Count > 0); //don't negate them
            Contract.Assert(lostCards.Count == gameController.CurrentPlayer.GameData.Resources.Current.Count / 2);
            var logHeader = new LoseCardsToSeven()
            {
                LostResources = lostCards,
                CanUndo = false
            };
            return gameController.PostMessage(logHeader, Catan.Proxy.ActionType.Normal);
        }
        public Task Do(IGameController gameController)
        {
            PlayerModel sentBy = gameController.NameToPlayer(this.SentBy);
            sentBy.GameData.Resources.GrantResources(LostResources.GetNegated());
            gameController.MainPageModel.Bank.GameData.Resources.GrantResources(LostResources);
            foreach (var player in gameController.MainPageModel.PlayingPlayers)
            {
                //
                //  stop if any player has more than 7 cards - the last person to get rid of 1/2 there cards will send the message to move the baron
                //
                if (player.GameData.Resources.Current.Count > 7)
                {
                    return Task.CompletedTask;
                }
            }

            //
            //  no player has more than 7 cards -- move the baron!
            return MustMoveBaronLog.PostLog(gameController, MoveBaronReason.Rolled7);
        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Replay(IGameController gameController)
        {
            PlayerModel sentBy = gameController.NameToPlayer(this.SentBy);
            sentBy.GameData.Resources.GrantResources(LostResources.GetNegated());
            gameController.MainPageModel.Bank.GameData.Resources.GrantResources(LostResources);
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }
    }
}