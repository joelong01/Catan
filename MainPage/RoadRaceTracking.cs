using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Catan10
{
    /// <summary>
    ///     This class keeps track of what player gets to a road count first
    ///     it keeps a full history in case a road is broken and to enable
    ///     a robust undo
    /// </summary>
    public class RoadRaceTracking
    {
        //
        //  given a road count, get the ordered list of players that have that many roads
        //  First() will be the one that got their first.
        //
        private readonly Dictionary<string, List<PlayerModel>> raceDictionary = new Dictionary<string, List<PlayerModel>>();

        //
        //  i'm trying to have the class take care of its own logging when state changes

        readonly ILog _log = null;
        // we can make lots of little changes - instead of logging all of them, we log at the end of them
        private string _beginState = "";

        public RoadRaceTracking(ILog log)
        {
            _log = log;
        }
        public RoadRaceTracking() { }

        public void AddPlayer(PlayerModel player, int roadCount)
        {

            if (!raceDictionary.TryGetValue(roadCount.ToString(), out List<PlayerModel> list))
            {
                list = new List<PlayerModel>();
                raceDictionary[roadCount.ToString()] = list;

            }

            if (!list.Contains(player))
            {
                list.Add(player);

            }


        }

        public void Undo(string oldVal, string newVal, PlayerModel player, ILogParserHelper logHelper, GameState state)
        {
            //  this.TraceMessage($"RoadRace Undo - setting To: {oldVal} from: {newVal}");
            this.Deserialize(oldVal, logHelper);
            _log.PostLogEntry(player, state, CatanAction.RoadTrackingChanged, false, LogType.Undo, -1, new LogRoadTrackingChanged(newVal, oldVal)); // note: new and old switched!
        }

        //
        //  Removes the player from the list and returns the old value it had
        //  -1 if it wasn't there.
        public int RemovePlayer(PlayerModel player, int roadCount)
        {
            int oldIndex = -1;

            if (raceDictionary.TryGetValue(roadCount.ToString(), out List<PlayerModel> list))
            {

                if (list != null)
                {
                    oldIndex = list.IndexOf(player);
                    list.Remove(player);


                    if (list.Count == 0)
                    {
                        raceDictionary.Remove(roadCount.ToString());

                    }
                }
            }
            return oldIndex;
        }


        public PlayerModel GetRaceWinner(int roadCount)
        {
            if (raceDictionary.TryGetValue(roadCount.ToString(), out List<PlayerModel> list))
            {
                Debug.Assert(list != null, "we shouldn't have added a null list");
                return list.First();
            }
            return null;
        }
        //
        //  serializes into the form of 
        //  roadCount=id1,id2,id3;roadCount=id2,id3;
        //  5=3,2,1;6=2,1
        //
        //  
        public string Serialize(bool useNames = false, string keySep = "/", string listSep = ",")
        {
            string s = "";
            foreach (KeyValuePair<string, List<PlayerModel>> kvp in raceDictionary)
            {
                if (kvp.Value == null)
                {
                    continue;
                }

                s += kvp.Key + "=";
                foreach (PlayerModel v in kvp.Value)
                {

                    s += useNames ? v.PlayerName : v.AllPlayerIndex.ToString();
                    s += listSep;


                }

                s += keySep;
            }


            return s;
        }
        public override string ToString()
        {
            return this.Serialize(true);
        }
        public void Reset()
        {
            raceDictionary.Clear();
        }
        public void Deserialize(string s, ILogParserHelper logParser, string keySep = "/", string listSep = ",")
        {
            Reset();
            string[] tokens = s.Split(keySep.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Count() < 1)
            {
                return;
            }

            foreach (string token in tokens)
            {
                // 5=1,2,3

                string[] ts = token.Split(new char[] { '=', listSep.ToCharArray()[0] }, StringSplitOptions.RemoveEmptyEntries);
                int roadCount = Int32.Parse(ts[0]);
                List<PlayerModel> list = new List<PlayerModel>();
                for (int i = 1; i < ts.Count(); i++) // yes 1, ts[0] is the roadCount
                {
                    int playerIndex = Int32.Parse(ts[i]);
                    PlayerModel player = logParser.GetPlayerData(playerIndex);
                    list.Add(player);
                }

                raceDictionary[roadCount.ToString()] = list;


            }

        }

        internal void BeginChanges()
        {
            _beginState = this.Serialize();
        }

        internal void EndChanges(PlayerModel player, GameState gameState, LogType logType = LogType.Normal)
        {
            //
            //  we put things back the way they were in the Undo function - this is only called because we are recalculating who we think should 
            //  get the longest road
            if (logType == LogType.Undo)
            {
                return;
            }

            string newState = this.Serialize();
            if (newState == _beginState) // no changes == don't log
            {
                return;
            }

            if (_log != null)
            {
                _log.PostLogEntry(player, gameState, CatanAction.RoadTrackingChanged, false, logType, -1, new LogRoadTrackingChanged(_beginState, newState));
            }
        }
    }


}
