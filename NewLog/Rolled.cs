using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Catan10
{


    /// <summary>
    ///     This class has all the data associated with a roll -- when a roll happens, we adjust both player stats and global stats.
    /// </summary>
    public class RolledModel : LogHeader
    {

        public int Rolled { get; set; } = -1;
        //
        //  there is a limitation in the system.text.json serializer in 3.0 where it will only serialize Dictionary<string, TValue>
        public Dictionary<string, PlayerRollData> PlayerToResources { get; set; }
        public RolledModel() { }
        public RolledModel(int n)
        {
            Rolled = n;
        }
        //
        //  compares 2 RolledModel objects for identity.  used for testing Redo.
        public bool Equals(RolledModel rolledModel)
        {
            if ( SentBy != rolledModel.SentBy || OldState != rolledModel.OldState ||
                Action != rolledModel.Action || Rolled != rolledModel.Rolled || PlayerToResources.Count != rolledModel.PlayerToResources.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, PlayerRollData> kvp in PlayerToResources)
            {
                bool exists = rolledModel.PlayerToResources.TryGetValue(kvp.Key, out PlayerRollData rollData);
                if (!exists) return false;
                if (kvp.Value.MaxRollNoResources != rolledModel.PlayerToResources[kvp.Key].MaxRollNoResources) return false;
                if (kvp.Value.ResourceList.Count != rollData.ResourceList.Count) return false;
                for (int i = 0; i < rollData.ResourceList.Count; i++)
                {
                    if (kvp.Value.ResourceList[i].Value != rollData.ResourceList[i].Value || kvp.Value.ResourceList[i].ResourceType != rollData.ResourceList[i].ResourceType ||
                        kvp.Value.ResourceList[i].BlockedByBaron != rollData.ResourceList[i].BlockedByBaron)
                    {
                        return false;
                    }
                }

            }

            return true;
        }
    }

    /// <summary>
    ///     this keeps track of the resources that are acquired because of the roll. there is a list of these in the PlayerRollData
    /// </summary>
    public class RollResourcesModel
    {
        public ResourceType ResourceType { get; set; }
        public int Value { get; set; }
        public bool BlockedByBaron { get; set; }
        public RollResourcesModel() { } // needed for deserialization
        public RollResourcesModel(ResourceType type, int value, bool blocked)
        {
            ResourceType = type;
            Value = value;
            BlockedByBaron = blocked;
        }
    }

    //
    //  this is the data about the player that we collect because a roll happened.
    //  in particular, there was no way to infer what the max roll with no resources should be without logging it 
    //  because we might have a tie (e.g. if the value for MaxRollsNoResources was 10 and we got an Undo where the current value was 10, 
    //  they might previously have had 10)
    public class PlayerRollData
    {
        public int MaxRollNoResources { get; set; } = 0;
        public List<RollResourcesModel> ResourceList { get; set; } = new List<RollResourcesModel>();
    }


    /// <summary>
    ///     Exposes a set of APIs that handle a roll (Do, Redo, Undo)
    /// </summary>
    public static class RolledController
    {

        /// <summary>
        /// 
        ///     the controller will fill out the RollModel which has the complete set of state impacted by this roll.
        /// 
        ///     1. push the roll
        ///     2. Update roll stats
        ///     3. Update resource counts
        ///     4. do Baron tracking 
        ///     5. set state to WaitingForNext (always comes after Roll)
        /// </summary>
        /// <returns></returns>
        public static RolledModel Do(MainPage page, int roll)
        {
            var currentPlayer = page.CurrentPlayer;
            RolledModel rolledModel = new RolledModel()
            {
               
                Rolled = roll,
                SentBy = currentPlayer.PlayerName,
                OldState = page.CurrentGameState,
                Action = (roll == 7) ? CatanAction.RolledSeven : CatanAction.Rolled
            };

            page.Rolls.Push(rolledModel.Rolled); // all the rolls...
            page.LastRoll = rolledModel.Rolled;
            page.UpdateGlobalRollStats(); // just math


            rolledModel.PlayerToResources = RolledController.GetResourceModelForRoll(page, rolledModel.Rolled);

            if (rolledModel.Rolled != 7)
            {
                currentPlayer.GameData.MovedBaronAfterRollingSeven = null;
                rolledModel.NewState = GameState.WaitingForNext;
            }
            else
            {
                currentPlayer.GameData.MovedBaronAfterRollingSeven = false;
                rolledModel.NewState = GameState.MustMoveBaron;
            }


            return rolledModel;



        }

        //
        //  given a roll and a multiplier (which should be 1 or -1), do a bunch of calculations.  this is the main "worker" function of the class
        //
        private static Dictionary<string, PlayerRollData> GetResourceModelForRoll(MainPage mainPage, int roll)
        {
            Dictionary<string, PlayerRollData> dict = new Dictionary<string, PlayerRollData>();
            // 
            // set up the dictionary and record MaxRollsNoResources

            foreach (var player in mainPage.MainPageModel.PlayingPlayers)
            {
                PlayerRollData rollData = new PlayerRollData()
                {
                    MaxRollNoResources = player.GameData.MaxNoResourceRolls,
                };
                dict[player.PlayerName] = rollData;
            }
            //
            //  get all information about the resources
            //
            foreach (TileCtrl tile in mainPage.GameContainer.AllTiles)
            {
                if (tile.Number == roll)
                {
                    tile.HighlightTile(mainPage.CurrentPlayer.BackgroundBrush); // shows what was rolled
                }
                else
                {
                    tile.StopHighlightingTile();
                }
                if (tile.Number != roll) continue;

                //
                //  the tile has the right number - see what settlements are on it
                foreach (BuildingCtrl building in tile.OwnedBuilding)
                {
                    if (building.Owner == null)
                    {
                        //
                        //  not owned -- continue
                        Debug.Assert(false, "Unowned buildings shouldn't be in the Owned Buildings collection!");
                        continue;
                    }
                    //
                    //  owned...it should be a settlement or a city
                    Debug.Assert(building.BuildingState == BuildingState.Settlement || building.BuildingState == BuildingState.City, "Owned buildings should be Settlements or Cities!");

                    //
                    //  get its value and add it to the dictionary that maps players to resources acquired
                    int value = building.BuildingState == BuildingState.Settlement ? 1 : 2;
                    var rrModel = new RollResourcesModel(tile.ResourceType, value, tile.HasBaron);
                    //
                    //  we added all the players above, so they need to be here.
                    dict[building.Owner.PlayerName].ResourceList.Add(rrModel);

                    /*
                        this updates 
                        1) the per turn resources a player acquired 
                        2) the toal # of resources a player has acquired (control notified via an event in PlayerResourceModel
                        3) the global count for each resource (via an event in PlayerResourceModel)
                        4) Cards Lost to Baron                        
                    */
                    building.Owner.GameData.UpdateResourceCount(rrModel, LogState.Normal);
                }
            }

            //
            //  go through players and update the good/bad roll count
            foreach (var player in mainPage.MainPageModel.PlayingPlayers)
            {
                if (player.GameData.PlayerTurnResourceCount.Total == 0)
                {
                    player.GameData.NoResourceCount++;
                    player.GameData.GoodRoll = false;
                }
                else
                {
                    player.GameData.RollsWithResource++;
                    player.GameData.NoResourceCount = 0;
                    player.GameData.GoodRoll = true;
                }
            }

            return dict;
        }


        public static bool Undo(MainPage page, RolledModel rollModel)
        {
            int roll = page.PopRoll();
            Debug.Assert(roll == rollModel.Rolled);
            page.UpdateGlobalRollStats();
            //
            //  we will go through each of the playing players to see if they are in the roll dictionary.  if they are, we'll undo 
            //  the resource allocation.  if they are not, we know they had a bad roll and will deal with it.
            //

            foreach (var player in page.MainPageModel.PlayingPlayers)
            {
                if (rollModel.PlayerToResources.TryGetValue(player.PlayerName, out PlayerRollData playerRollData) == true)
                {
                    if (playerRollData.ResourceList.Count > 0)
                    {
                        player.GameData.RollsWithResource--;
                        foreach (RollResourcesModel resourceModel in playerRollData.ResourceList)
                        {
                            player.GameData.UpdateResourceCount(resourceModel, LogState.Undo);
                        }
                    }
                    else
                    {
                        player.GameData.NoResourceCount--;
                    }

                    player.GameData.MaxNoResourceRolls = playerRollData.MaxRollNoResources;
                }
                else
                {
                    throw new Exception("We should have *all* players in the rollModel!");
                }
            }

            //
            //  reset the flag that says they need to move the baron
            if (rollModel.Rolled != 7)
            {
                page.CurrentPlayer.GameData.MovedBaronAfterRollingSeven = null;
            }
            else
            {
                page.CurrentPlayer.GameData.MovedBaronAfterRollingSeven = false;

            }

            //
            //  get rid of any highlighting
            foreach (TileCtrl tile in page.GameContainer.AllTiles)
            {

                tile.StopHighlightingTile();
            }

            return true;
        }

        internal static void Redo(MainPage page, RolledModel model)
        {
            var newModel = RolledController.Do(page, model.Rolled);
            if (newModel.Equals(model) == false)
            {
                throw new Exception("new and old Roll models must match on Redo!");
            }
        }
    }
}
