using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class PlayDevCardLog : LogHeader, ILogController
    {
        public DevCardType DevCardType { get; set; }

        /// <summary>
        ///     For Monopoly, this is the resource requested
        ///     For YoP, these are the two Cards picked
        ///     for RB, this is empty
        ///     BuyOrUpgradeKnight is more complicated and is its own message
        /// </summary>
        public TradeResources TradeResources { get; set; }

        public static async Task PostLog(IGameController gameController, DevCardType devCardType, TradeResources resources)
        {
            Contract.Assert(gameController.CurrentPlayer.GameData.Resources.ThisTurnsDevCard.DevCardType == DevCardType.None);
            Contract.Assert(devCardType == DevCardType.YearOfPlenty || devCardType == DevCardType.Monopoly || devCardType == DevCardType.RoadBuilding);

            PlayDevCardLog logHeader = new PlayDevCardLog()
            {
                CanUndo = false, // BIG DEAL:  stops the undo tree..
                DevCardType = devCardType,
                Action = CatanAction.PlayedDevCard,
                SentBy = gameController.CurrentPlayer,
                TradeResources = resources
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        /// <summary>
        ///     this needs to update the gamestate to reflect that a card was just played.  it doesn't do anything about the semantic of playing the card.  that is a seperate message.
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public Task Do(IGameController gameController)
        {
            Contract.Assert(DevCardType == DevCardType.YearOfPlenty || DevCardType == DevCardType.Monopoly || DevCardType == DevCardType.RoadBuilding);
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForNext);

            var sentBy = gameController.NameToPlayer(this.SentBy);

            Contract.Assert(sentBy.GameData.Resources.ThisTurnsDevCard.DevCardType == DevCardType.None);
            Contract.Assert(sentBy == gameController.CurrentPlayer); // only current players can play dev cards


            DevCardModel localDevCard = null;
            //
            //  find the first one and make sure we have it locally
            foreach (var card in sentBy.GameData.Resources.AvailableDevCards)
            {
                if (card.DevCardType == this.DevCardType)
                {
                    localDevCard = card;
                    break;
                }
            }

            Contract.Assert(localDevCard != null);

            sentBy.GameData.Resources.AvailableDevCards.Remove(localDevCard);
            sentBy.GameData.Resources.PlayedDevCards.Add(localDevCard);
            sentBy.GameData.Resources.ThisTurnsDevCard.DevCardType = localDevCard.DevCardType;
            localDevCard.Played = true;

            if (DevCardType == DevCardType.YearOfPlenty)
            {
                sentBy.GameData.Resources.GrantResources(this.TradeResources);
            }
            else if (DevCardType == DevCardType.RoadBuilding)
            {
                sentBy.GameData.Resources.GrantEntitlement(Entitlement.Road);
                sentBy.GameData.Resources.GrantEntitlement(Entitlement.Road);
            }
            else if (DevCardType == DevCardType.Monopoly)
            {
                Contract.Assert(TradeResources.Count == 1);
                ResourceType pickedResourceType = ResourceType.None;
                foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
                {
                    if (this.TradeResources.GetCount(rt) != 0)
                    {
                        pickedResourceType = rt;
                        break;
                    }
                }

                Contract.Assert(pickedResourceType != ResourceType.None);

                int total = 0;

                foreach (var player in gameController.PlayingPlayers)
                {
                    player.GameData.Resources.ResourcesThisTurn.Reset();
                    if (player == sentBy) continue;

                    int count = player.GameData.Resources.CurrentResources.CountForResource(pickedResourceType);
                    TradeResources lost = new TradeResources();
                    lost.AddResource(pickedResourceType, -count);
                    player.GameData.Resources.GrantResources(lost);
                    player.GameData.Resources.ResourcesLostToMonopoly += lost;
                    player.GameData.Resources.StolenResource = pickedResourceType;
                    total += count;
                }
                TradeResources gained = new TradeResources();                
                gained.AddResource(pickedResourceType, total);
                sentBy.GameData.Resources.GrantResources(gained);
            }
            else
            {
                this.TraceMessage($"{DevCardType}");
                Contract.Assert(false, "What kind of devcard is it?");
            }

            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }

        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            throw new System.NotImplementedException();
        }
    }
}