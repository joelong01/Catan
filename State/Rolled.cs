using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Catan10
{
    public interface ILogController
    {
        // executes the task and returns the serialized log string
        LogHeader Do();
        // given a log string, undo the action
        Task<bool> Undo(string logLineJson);
        Task<bool> Undo(LogHeader header);
        // given a log string, redo the action
        Task Redo(string logLineJson);
        void Redo(LogHeader t);

    }

    public interface ILogHeader
    {
        int PlayerIndex { get; set; }
        string PlayerName { get; set; }
        GameState OldState { get; set; }
        GameState NewState { get; set; }
        CatanAction Action { get; set; }
    }



    public class LogHeader : ILogHeader
    {
        public int PlayerIndex { get; set; }
        public string PlayerName { get; set; }
        public GameState OldState { get; set; }
        public GameState NewState { get; set; }
        public CatanAction Action { get; set; }


        [JsonIgnore]
        public MainPage Page { get; internal set; } = null;
        [JsonIgnore]
        public PlayerModel Player { get; internal set; } = null;

        public LogHeader()
        {

        }
        public LogHeader(MainPage page, PlayerModel playerData, GameState newState, CatanAction a)
        {
            PlayerIndex = playerData.AllPlayerIndex;
            PlayerName = playerData.PlayerName;
            Player = playerData;
            Page = page;
            NewState = newState;
            OldState = Page.GameState;
            Action = a;
            Page = page;

        }


    }

    public class RolledModel : LogHeader
    {

        public int Rolled { get; set; } = -1;
        //
        //  there is a limitation in the system.text.json serializer in 3.0 where it will only serialize Dictionary<string, TValue>
        public Dictionary<string, List<RollResourcesModel>> PlayerToResources { get; set; }
        public RolledModel() { }
        public RolledModel(int n)
        {
            Rolled = n;
        }
        public RolledModel(int n, Dictionary<string, List<RollResourcesModel>> playerResourceDictionary)
        {
            Rolled = n;
            PlayerToResources = playerResourceDictionary;
        }

        public RolledModel(MainPage page, PlayerModel playerData, CatanAction action, int roll)
        {
            PlayerIndex = playerData.AllPlayerIndex;
            PlayerName = playerData.PlayerName;
            Player = playerData;
            Page = page;
            OldState = Page.GameState;
            Action = action;
            Rolled = roll;

        }
    }

    public class RollResourcesModel
    {
        public ResourceType ResourceType { get; set; }
        public int Value { get; set; }
        public bool BlockedByBaron { get; set; }
        public RollResourcesModel() { }
        public RollResourcesModel(ResourceType type, int value, bool blocked)
        {
            ResourceType = type;
            Value = value;
            BlockedByBaron = blocked;
        }
    }

    public class RolledController
    {
        private RolledModel Model { get; set; }
        private MainPage Page { get; set; } = null;
        public RolledController(MainPage p, int roll)
        {
            Model = new RolledModel(p, p.CurrentPlayer, CatanAction.Rolled, roll);
            Page = p;
        }

        public RolledController(MainPage p)
        {
            Page = p;
        }

        //
        //  used by Undo / redo
        public RolledController(MainPage p, RolledModel model)
        {
            Page = p;
            Model = model;
        }

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
        public LogHeader Do()
        {


            GameState newState;

            Page.Rolls.Push(Model.Rolled); // all the rolls...
            Page.LastRoll = Model.Rolled;
            Page.UpdateGlobalRollStats(); // just math

            Model.PlayerToResources = GetResourceModelForRoll(Model.Rolled);

            if (Model.Rolled != 7)
            {
                Page.CurrentPlayer.GameData.MovedBaronAfterRollingSeven = null;
                newState = GameState.WaitingForNext;
            }
            else
            {
                Page.CurrentPlayer.GameData.MovedBaronAfterRollingSeven = false;
                newState = GameState.MustMoveBaron;
            }
            
            Model.NewState = newState;
            return Model;

        }

        //
        //  given a roll and a multiplier (which should be 1 or -1), do a bunch of calculations
        private Dictionary<string, List<RollResourcesModel>> GetResourceModelForRoll(int roll)
        {
            Dictionary<string, List<RollResourcesModel>> dict = new Dictionary<string, List<RollResourcesModel>>();
            //
            //  get all information about the resources
            //
            foreach (TileCtrl tile in Page.CurrentGame.AllTiles)
            {
                tile.HighlightTile(tile.Number == roll); // shows what was rolled
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
                    if (dict.TryGetValue(building.Owner.PlayerName, out List<RollResourcesModel> list) == true)
                    {
                        list.Add(rrModel);
                    }
                    else
                    {
                        var lst = new List<RollResourcesModel>{rrModel};
                        dict[building.Owner.PlayerName] = lst;
                    }

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
            //  go through players
            foreach (var player in Page.PlayingPlayers)
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



        public Task Redo(string jsonString)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Undo(string logLineJson)
        {
            throw new NotImplementedException();
        }

        public void Redo(LogHeader t)
        {
            throw new NotImplementedException();
        }

        private PlayerModel LookupPlayerByName(string playerName)
        {
            foreach (PlayerModel p in Page.PlayingPlayers)
            {
                if (p.PlayerName == playerName)
                    return p;
            }

            throw new Exception("Log contained a playername that was not in the PlayingPlayers collection!");
        }

        public async Task<bool> Undo(LogHeader logModel)
        {
            int roll = Page.PopRoll();
            RolledModel rollModel = logModel as RolledModel;
            Debug.Assert(roll == rollModel.Rolled);
            Page.UpdateGlobalRollStats();

            foreach (KeyValuePair<string, List<RollResourcesModel>> kvp in rollModel.PlayerToResources)
            {
                var player = LookupPlayerByName(kvp.Key);
                foreach (RollResourcesModel resourceModel in kvp.Value)
                {
                    player.GameData.UpdateResourceCount(resourceModel, LogState.Undo);
                }
            }


            if (Model.Rolled != 7)
            {
                Page.CurrentPlayer.GameData.MovedBaronAfterRollingSeven = null;
            }
            else
            {
                Page.CurrentPlayer.GameData.MovedBaronAfterRollingSeven = false;

            }
            return true;
        }


    }
}
