using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

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
            Contract.Assert(lostCards.Count == gameController.TheHuman.GameData.Resources.CurrentResources.Count / 2);
            var logHeader = new LoseCardsToSeven()
            {
                LostResources = lostCards,
                CanUndo = false
            };
            return gameController.PostMessage(logHeader, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            PlayerModel sentBy = gameController.NameToPlayer(this.SentBy);
            sentBy.GameData.Resources.GrantResources(LostResources.GetNegated());
            gameController.MainPageModel.Bank.GameData.Resources.GrantResources(LostResources);
            
            foreach (var player in gameController.MainPageModel.PlayingPlayers)
            {
                //
                //   7/29/2020:  if the player who had to discard cards has 16 or more cards, they will be left with more than 7.
                //               so go back through the log and skip people that have already discarded cards. 

                if (PlayerGaveUpCardsThisTurn(player, gameController))
                {
                    this.TraceMessage($"{player} game up cards this turn");
                    continue;
                }
                
                this.TraceMessage($"{player} did NOT give up cards this turn.  Has {player.GameData.Resources.CurrentResources.Count} cards");

                //
                //  stop if any player has more than 7 cards - the last person to get rid of 1/2 there cards will send the message to move the baron
                //
                if (player.GameData.Resources.CurrentResources.Count > 7)
                {
                     await DefaultTask;
                }
            }

            //
            //  no player has more than 7 cards -- move the baron!
            await MustMoveBaronLog.PostLog(gameController, MoveBaronReason.Rolled7);
        }

        public bool PlayerGaveUpCardsThisTurn(PlayerModel player, IGameController gameController)
        {
            for (int i = gameController.Log.MessageLog.Count - 1; i>=0; i--)
            {
                CatanMessage message = gameController.Log.MessageLog[i];
                if (message.DataTypeName == typeof(LoseCardsToSeven).FullName)
                {
                    if (message.From == player.PlayerName)
                    {
                        return true;
                    }
                }

                if (message.DataTypeName == typeof(WaitingForRollToWaitingForNext).FullName)
                {
                    return false;
                }
            }

            return false;
        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public async Task Replay(IGameController gameController)
        {
            PlayerModel sentBy = gameController.NameToPlayer(this.SentBy);
            sentBy.GameData.Resources.GrantResources(LostResources.GetNegated());
            gameController.MainPageModel.Bank.GameData.Resources.GrantResources(LostResources);
             await DefaultTask;
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }
    }
}