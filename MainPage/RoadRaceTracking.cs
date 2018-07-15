﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

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
        private readonly Dictionary<int, List<PlayerData>> raceDictionary = new Dictionary<int, List<PlayerData>>();

        //
        //  i'm trying to have the class take care of its own logging when state changes
        ILog _log = null;
        // we can make lots of little changes - instead of logging all of them, we log at the end of them
        private string _beginState = "";
        
        public RoadRaceTracking(ILog log)
        {
            _log = log;
        }

        public void AddPlayer(PlayerData player, int roadCount)
        {
            
            if (!raceDictionary.TryGetValue(roadCount, out List<PlayerData> list))
            {
                list = new List<PlayerData>();
                raceDictionary[roadCount] = list;

            }

            if (!list.Contains(player))
            {
                list.Add(player);
                
            }

            
        }

        public void Undo(string oldVal, string newVal, PlayerData player, ILogParserHelper logHelper, GameState state)
        {
          //  this.TraceMessage($"RoadRace Undo - setting To: {oldVal} from: {newVal}");
            this.Deserialize(oldVal, logHelper);
            _log.AddLogEntry(player, state, CatanAction.RoadTrackingChanged, false, LogType.Undo, -1, new LogRoadTrackingChanged(newVal, oldVal)); // note: new and old switched!
        }

        //
        //  Removes the player from the list and returns the old value it had
        //  -1 if it wasn't there.
        public int RemovePlayer(PlayerData player, int roadCount)
        {
            int oldIndex = -1;
            
            if (raceDictionary.TryGetValue(roadCount, out List<PlayerData> list))
            {
                
                if (list != null)
                {
                    oldIndex = list.IndexOf(player);
                    list.Remove(player);
                    
                    
                    if (list.Count == 0)
                    {
                        raceDictionary.Remove(roadCount);
                        
                    }
                }
            }
            return oldIndex;
        }

       
        public PlayerData GetRaceWinner(int roadCount)
        {
            if (raceDictionary.TryGetValue(roadCount, out List<PlayerData> list))
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
            foreach (var kvp in raceDictionary)
            {
                if (kvp.Value == null) continue;
                s += kvp.Key + "=";
                foreach (var v in kvp.Value)
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
                return;
            

            foreach (var token in tokens)
            {
                // 5=1,2,3

                var ts = token.Split(new char[] { '=', listSep.ToCharArray()[0] }, StringSplitOptions.RemoveEmptyEntries);
                int roadCount = Int32.Parse(ts[0]);
                List<PlayerData> list = new List<PlayerData>();
                for (int i = 1; i < ts.Count(); i++) // yes 1, ts[0] is the roadCount
                {
                    int playerIndex = Int32.Parse(ts[i]);
                    PlayerData player = logParser.GetPlayerData(playerIndex);
                    list.Add(player);
                }

                raceDictionary[roadCount] = list;


            }

        }

        internal void BeginChanges()
        {
            _beginState = this.Serialize();
        }

        internal void EndChanges(PlayerData player, GameState gameState, LogType logType=LogType.Normal)
        {
            //
            //  we put things back the way they were in the Undo function - this is only called because we are recalculating who we think should 
            //  get the longest road
            if (logType == LogType.Undo)
                return;
            string newState = this.Serialize();
            if (newState == "") return;
            if (newState == _beginState)
                return;
        //    this.TraceMessage($"RoadRace - old:{_beginState} new: {newState}");
            _log.AddLogEntry(player, gameState, CatanAction.RoadTrackingChanged, false, logType, -1, new LogRoadTrackingChanged(_beginState, newState));
        }
    }

   
}
