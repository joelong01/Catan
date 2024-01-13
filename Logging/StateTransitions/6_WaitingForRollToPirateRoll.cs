using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
namespace Catan10
{
    internal class WaitingForRollToPirateRoll : LogHeader, ILogController
    {
        public RollModel RollModel { get; set; }
        public List<Guid> Victims { get; set; }
        public List<Guid> Winners { get; set; }
        public List<KnightCtrl> ActiveKnights { get; set; }
        internal static async Task PostRollMessage(IGameController gameController, RollModel roll)
        {
            // this is "if there is an invasion, who  win/loses?"
            // doing it this way guarantees that Do and Undo are iterating over the same data
            var (winners, victims) = GetWinnersAndLosers(gameController);

            var activeKnights = new List<KnightCtrl>();
            // if there is an invasion, we also need to know the active knights
            foreach (var player in gameController.PlayingPlayers)
            {
                player.GameData.CK_Knights.ForEach(knight =>
                {
                    if (knight.Activated) activeKnights.Add(knight);
                });
            }

            //
            //  if it isn't a pirate dice, just take care of the roll
            if (roll.SpecialDice != SpecialDice.Pirate)
            {
                await WaitingForRollToWaitingForNext.PostRollMessage(gameController, roll);
                return;
            }

            var logEntry = new WaitingForRollToPirateRoll()
            {
                NewState = GameState.HandlePirates,
                RollModel = roll,
                Winners = winners,
                Victims = victims,
                ActiveKnights = activeKnights
            };

            await gameController.PostMessage(logEntry, ActionType.Normal);
        }
        /// <summary>
        ///     checks to see if players won the invasion.  returns a list of potential victims and winerrs
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public static (List<Guid> winners, List<Guid> victims) GetWinnersAndLosers(IGameController gameController)
        {
            var victims = new List<Guid>();
            var winners = new List<Guid>();
            bool playersLost = gameController.MainPageModel.TotalCities > gameController.MainPageModel.TotalKnightRanks;

            // Using LINQ to find the lowest and highest knight rank
            int lowestKnightRank = gameController.PlayingPlayers.Min(player => player.GameData.TotalKnightRank);
            int highestKnightRank = gameController.PlayingPlayers.Max(player => player.GameData.TotalKnightRank);

            foreach (var player in gameController.PlayingPlayers)
            {
                int totalKnightRank = player.GameData.TotalKnightRank;

                // Logic for victims
                if (playersLost && totalKnightRank == lowestKnightRank && player.GameData.Cities.Count > 0)
                {
                    victims.Add(player.PlayerIdentifier);
                }

                // Logic for winners
                if (totalKnightRank == highestKnightRank)
                {
                    if (playersLost && highestKnightRank > lowestKnightRank)
                    {
                        winners.Add(player.PlayerIdentifier);
                    }
                    else if (!playersLost)
                    {
                        winners.Add(player.PlayerIdentifier);
                    }
                }
            }

            return (winners, victims);
        }

        /// <summary>
        ///     1. this should only be called for SpecialDice.Pirate
        ///     2. increment the Invasion Step counter
        ///     3. if we are less than the steps for an invasion, just go to WaitingforNext
        ///     4. if we are at the Invasion Steps
        ///         a) move the Baron to the main board.  TODO: make this an option
        ///         b) get the winners and losers for the invasion
        ///         c) if there are losers, grant them intitlements and move to the state for them to select destroying a city
        ///         d) the winners get a VP (if only one winner) or a "Pick Any Dev Card" 
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public async Task Do(IGameController gameController)
        {
            if (this.RollModel.SpecialDice != SpecialDice.Pirate) return;

            int count = gameController.InvasionData.NextStep();
            if (count < gameController.InvasionData.INVASION_STEPS)
            {
                //
                //  we've moved the ship and now we just deal with the rollModel

                await WaitingForRollToWaitingForNext.PostRollMessage(gameController, this.RollModel);
                return;
            }


            Debug.Assert(count == gameController.InvasionData.INVASION_STEPS);
            gameController.InvasionData.ShowBaron = false;
            gameController.GameContainer.CurrentGame.HexPanel.ShowBaron();


            if (Victims.Count > 0)
            {
                foreach (var victimId in Victims)
                {
                    gameController.PlayerFromId(victimId).GameData.Resources.GrantEntitlement(Entitlement.DestroyCity);
                }
               
                await DestroyCity_Next.PostLog(gameController);
            }
            if (Winners.Count == 1)
            {
                var tr = new TradeResources();
                tr.AddResource(ResourceType.VictoryPoint, 1);
                gameController.PlayerFromId(Winners[0]).GameData.Resources.GrantResources(tr);
                gameController.PlayerFromId(Winners[0]).GameData.Resources.VictoryPoints++; //TODO: put this into GrantResource
                gameController.PlayerFromId(Winners[0]).GameData.VictoryPoints--; // TODO!

            }
            else
            {
                foreach (var id in Winners)
                {
                    var tr = new TradeResources();
                    tr.AddResource(ResourceType.AnyDevCard, 1);
                    gameController.PlayerFromId(id).GameData.Resources.GrantResources(tr);
                }
            }


            /**
             * after the invasion, all knights go inactive
             */
            ActiveKnights.ForEach(knight => knight.Activated = false);
           
            if (Victims.Count == 0)
            {
                await WaitingForRollToWaitingForNext.PostRollMessage(gameController, this.RollModel);
            }
            await Task.Delay(0);

        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public Task Replay(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public async Task Undo(IGameController gameController)
        {

            Debug.Assert(RollModel.SpecialDice == SpecialDice.Pirate);

            int step = gameController.InvasionData.PreviousStep();
            if (step == gameController.InvasionData.INVASION_STEPS - 1 && gameController.InvasionData.TotalInvasions == 0)
            {
                gameController.InvasionData.ShowBaron = true;
                gameController.GameContainer.CurrentGame.HexPanel.HideBaron();

                if (Winners.Count == 1)
                {
                    var tr = new TradeResources();
                    tr.AddResource(ResourceType.VictoryPoint, -1);
                    gameController.PlayerFromId(Winners[0]).GameData.Resources.GrantResources(tr);
                    gameController.PlayerFromId(Winners[0]).GameData.Resources.VictoryPoints--; // TODO!
                    gameController.PlayerFromId(Winners[0]).GameData.VictoryPoints--; // TODO!

                }
                else
                {
                    foreach (var id in Winners)
                    {
                        var tr = new TradeResources();
                        tr.AddResource(ResourceType.AnyDevCard, -1);
                        gameController.PlayerFromId(id).GameData.Resources.GrantResources(tr);
                    }
                }

                if (Victims.Count > 0)
                {
                    foreach (var victimId in Victims)
                    {
                        gameController.PlayerFromId(victimId).GameData.Resources.RevokeEntitlement(Entitlement.DestroyCity);
                    }
                }

                ActiveKnights.ForEach(knight => knight.Activated = true);

            }
            await Task.Delay(0);
        }
    }
}
