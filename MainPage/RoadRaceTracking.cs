using System.Collections.Generic;
using System.Diagnostics;

using Catan.Proxy;
using Catan10.CatanService;

namespace Catan10
{
    /// <summary>
    ///     This class keeps track of what player gets to a road count first
    ///     it keeps a full history in case a road is broken and to enable
    ///     a robust undo
    /// </summary>
    public class RoadRaceTracking
    {
        // we can make lots of little changes - instead of logging all of them, we log at the end of them
        private string _beginState = "";

        private IGameController GameController = null;

        internal void BeginChanges()
        {
            _beginState = this.Serialize();
        }

        internal void EndChanges(PlayerModel player, GameState gameState)
        {
            string newState = this.Serialize();
            if (newState == _beginState) // no changes == don't log
            {
                return;
            }
        }

        //
        //  given a road count, get the ordered list of players that have that many roads
        //  First() will be the one that got their first.
        //
        public Dictionary<string, List<PlayerModel>> RaceDictionary { get; set; } = new Dictionary<string, List<PlayerModel>>();

        public RoadRaceTracking(IGameController gameController)
        {
            GameController = gameController;
        }

        public RoadRaceTracking()
        {
        }

        public static RoadRaceTracking Deserialize(string json)
        {
            return CatanSignalRClient.Deserialize<RoadRaceTracking>(json);
        }

        public void AddPlayer(PlayerModel player, int roadCount)
        {
            if (!RaceDictionary.TryGetValue(roadCount.ToString(), out List<PlayerModel> list))
            {
                list = new List<PlayerModel>();
                RaceDictionary[roadCount.ToString()] = list;
            }

            if (!list.Contains(player))
            {
                list.Add(player);
            }
        }

        public PlayerModel GetRaceWinner(int roadCount)
        {
            if (RaceDictionary.TryGetValue(roadCount.ToString(), out List<PlayerModel> list))
            {
                Debug.Assert(list != null, "we shouldn't have added a null list");
                return list.First();
            }
            return null;
        }

        //
        //  Removes the player from the list and returns the old value it had
        //  -1 if it wasn't there.
        public int RemovePlayer(PlayerModel player, int roadCount)
        {
            int oldIndex = -1;

            if (RaceDictionary.TryGetValue(roadCount.ToString(), out List<PlayerModel> list))
            {
                if (list != null)
                {
                    oldIndex = list.IndexOf(player);
                    list.Remove(player);

                    if (list.Count == 0)
                    {
                        RaceDictionary.Remove(roadCount.ToString());
                    }
                }
            }
            return oldIndex;
        }

        public void Reset()
        {
            RaceDictionary.Clear();
        }

        //
        //  serializes into the form of
        //  roadCount=id1,id2,id3;roadCount=id2,id3;
        //  5=3,2,1;6=2,1
        //
        //
        public string Serialize(bool indent = false)
        {
            return CatanSignalRClient.Serialize(this, indent);
        }

        public override string ToString()
        {
            return this.Serialize(false);
        }
    }
}
