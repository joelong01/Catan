using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Catan.Proxy;
namespace Catan10
{
    //   an object that encapsulates an action that has happned in the game
    public class LogEntry
    {
        public LogType LogType { get; set; } = LogType.Normal;

        public GameState GameState { get; set; }
        public PlayerModel PlayerData { get; set; }
        public string PlayerDataString { get; set; } = "";
        public CatanAction Action { get; set; }
        public int Number { get; set; } = -1;
        public bool StopProcessingUndo { get; set; } = true; // when an action takes place, does it show up in the UI?  Cononical example:  supplemental also does changes current player.  when you undo, you need to undo both records        
        public object Tag { get; set; } = null;
        private string TagAsString { get; set; } = "";
        public string MemberName { get; set; } = "";
        public int LineNumber { get; set; } = -1;
        public string Path { get; set; } = "";


        public string PlayerName
        {
            get
            {
                if (PlayerData == null)
                {
                    return "<none>";
                }

                return PlayerData.PlayerName;
            }
        }

        public LogEntry(PlayerModel p, GameState s, CatanAction a, int n, bool stopUndo, LogType type = LogType.Normal, object tag = null, [CallerMemberName] string cmn = "", [CallerLineNumber] int cln = 0, [CallerFilePath] string cfp = "")
        {
            PlayerData = p;
            GameState = s;
            Action = a;
            Number = n;
            Tag = tag;
            LogType = type;
            StopProcessingUndo = stopUndo;
            MemberName = cmn;
            LineNumber = cln;
            Path = cfp;

        }

        static readonly private string[] _serializeProperties = new string[] { "PlayerDataString", "GameState", "Action", "Number", "StopProcessingUndo", "TagAsString", "MemberName", "LineNumber" }; // "Path" removed
        /// <summary>
        ///     this is mostly used for Console and debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (TagAsString == "" && Tag != null)
            {
                TagAsString = Tag.ToString();
            }

            if (PlayerData != null && PlayerDataString == "")
            {
                PlayerDataString = String.Format($"{PlayerData.PlayerName}");
            }

            return $" State:{GameState,-30} | Action:{Action,-25} | LogType:{LogType,-10}  | {PlayerDataString,-5} | #:{Number,-5} Tag:{TagAsString,-20}";


        }

        /// <summary>
        ///     this is what is put onto disk for the LogEntry
        /// </summary>
        public string Serialize()
        {
            if (TagAsString == "" && Tag != null)
            {
                TagAsString = Tag.ToString();
            }

            if (PlayerData != null)
            {
                PlayerDataString = String.Format($"{PlayerData.PlayerName}.{PlayerData.AllPlayerIndex}.{PlayerData.PlayerPosition}");
            }

            return StaticHelpers.SerializeObject<LogEntry>(this, _serializeProperties, "=", "|");
        }

        public LogEntry(string s, ILogParserHelper parseHelper)
        {
            StaticHelpers.DeserializeObject<LogEntry>(this, s, "=", "|");
            ParsePlayer(PlayerDataString, parseHelper);
            ParseTag(TagAsString, parseHelper);
        }


        private void ParseTag(string val, ILogParserHelper parseHelper)
        {
            switch (Action)
            {

                case CatanAction.ChangedPlayer:
                    Tag = new LogChangePlayer(val);
                    break;
                case CatanAction.AssignedBaron:
                case CatanAction.PlayedKnight:
                case CatanAction.AssignedPirateShip:
                    Tag = new LogBaronOrPirate(val, parseHelper);
                    break;
                case CatanAction.UpdatedRoadState:
                    Tag = new LogRoadUpdate(val, parseHelper);
                    break;
                case CatanAction.UpdateBuildingState:
                    Tag = new LogBuildingUpdate(val, parseHelper);
                    break;
                case CatanAction.AddPlayer:
                    Tag = Enum.Parse(typeof(PlayerPosition), val);
                    break;
                case CatanAction.AssignHarbors:
                case CatanAction.AssignRandomTiles:
                case CatanAction.RandomizeTiles:
                    Tag = LogList<int>.CreateAndParse(val);
                    break;
                case CatanAction.ChangedState:
                    Tag = new LogStateTranstion(val);
                    break;
                case CatanAction.CardsLost:
                    Tag = new LogCardsLost(val);
                    break;
                case CatanAction.SetFirstPlayer:
                    Tag = new LogSetFirstPlayer(val);
                    break;
                case CatanAction.RoadTrackingChanged:
                    Tag = new LogRoadTrackingChanged(val);
                    break;
                case CatanAction.ChangedPlayerProperty:
                    Tag = new LogPropertyChanged(val);
                    break;
                case CatanAction.AddResourceCount:
                    Tag = new LogResourceCount(val);
                    break;
                default:
                    break;
            }
        }



        private void ParsePlayer(string player, ILogParserHelper parseHelper)
        {
            //
            //  should be in the form of "Joe.0.BottomRight
            //  
            string[] tokens = player.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
            {
                // this is a normal occurance when there isn't a player, like the beginning of the game
                return;
            }


            if (Int32.TryParse(tokens[1], out int index))
            {
                PlayerData = parseHelper.GetPlayerData(index);
                if (tokens.Length > 2)
                {
                    if (Enum.TryParse<PlayerPosition>(tokens[2], out PlayerPosition pos))
                    {
                        PlayerData.PlayerPosition = pos;
                    }
                    else
                    {
                        this.TraceMessage($"No player Position in PlayerData: {PlayerData}");
                    }

                }
            }
        }



        public bool Undoable => (Action != CatanAction.ChangedState);


    }

    public class LogSetFirstPlayer
    {
        public int FirstPlayerIndex { get; set; }
        public LogSetFirstPlayer(int playerIndex)
        {
            FirstPlayerIndex = playerIndex;
        }
        public LogSetFirstPlayer(string saved)
        {
            Deserialize(saved);
        }

        private readonly string[] _serializedProperties = new string[] { "FirstPlayerIndex" };

        public override string ToString()
        {
            return StaticHelpers.SerializeObject<LogSetFirstPlayer>(this, _serializedProperties, ":", ",");

        }

        public void Deserialize(string saved)
        {
            StaticHelpers.DeserializeObject<LogSetFirstPlayer>(this, saved, ":", ",");
        }
    }

    public class LogChangePlayer
    {
        public int From { get; set; } = -1;
        public int To { get; set; } = -1;
        public List<int> OldRandomGoldTiles { get; set; } = new List<int>();
        public List<int> NewRandomGoldTiles { get; set; } = new List<int>();
        public GameState OldGameState { get; set; }
        private readonly string[] _serializedProperties = new string[] { "From", "To", "NewRandomGoldTiles", "OldRandomGoldTiles", "OldGameState" };
        public LogChangePlayer(int oldIdx, int newIdx, GameState oldState, List<int> rgtOld, List<int> rgtNew)
        {
            From = oldIdx;
            To = newIdx;
            OldGameState = oldState; // new game state kept in LogEntry
            OldRandomGoldTiles = rgtOld;
            NewRandomGoldTiles = rgtNew;
        }
        public LogChangePlayer(string saved)
        {
            Deserialize(saved);
        }

        public override string ToString()
        {
            return StaticHelpers.SerializeObject<LogChangePlayer>(this, _serializedProperties, ":", ",");

        }

        public void Deserialize(string saved)
        {
            StaticHelpers.DeserializeObject<LogChangePlayer>(this, saved, ":", ",");
        }

    }
    public class LogStateTranstion
    {
        public GameState OldState { get; set; } = GameState.Uninitialized;
        public GameState NewState { get; set; } = GameState.Uninitialized;
        public List<int> RandomGoldTiles { get; set; } = new List<int>();

        private readonly string[] _serializedProperties = new string[] { "OldState", "NewState", "RandomGoldTiles" };

        public LogStateTranstion(GameState old, GameState newState)
        {
            OldState = old;
            NewState = newState;

        }

        public LogStateTranstion(string saved)
        {
            Deserialize(saved);
        }

        public override string ToString()
        {
            return StaticHelpers.SerializeObject<LogStateTranstion>(this, _serializedProperties, ":", ",");

        }

        public void Deserialize(string saved)
        {
            StaticHelpers.DeserializeObject<LogStateTranstion>(this, saved, ":", ",");
        }

    }

    public class LogCardsLost
    {
        public int OldVal { get; set; } = 0;
        public int NewVal { get; set; } = 0;


        private readonly string[] _serializedProperties = new string[] { "OldVal", "NewVal" };

        public LogCardsLost(int old, int newState)
        {
            OldVal = old;
            NewVal = newState;

        }

        public LogCardsLost(string saved)
        {
            Deserialize(saved);
        }

        public override string ToString()
        {
            return StaticHelpers.SerializeObject<LogCardsLost>(this, _serializedProperties, ":", ",");

        }

        public void Deserialize(string saved)
        {
            StaticHelpers.DeserializeObject<LogCardsLost>(this, saved, ":", ",");
        }

    }

    public class LogResourceCount
    {
        public int OldVal { get; set; } = 0;
        public int NewVal { get; set; } = 0;
        public ResourceType ResourceType { get; set; } = ResourceType.None;


        private readonly string[] _serializedProperties = new string[] { "OldVal", "NewVal", "ResourceType" };

        public LogResourceCount(int oldVal, int newVal, ResourceType resType)
        {
            OldVal = oldVal;
            NewVal = newVal;
            ResourceType = resType;

        }

        public LogResourceCount(string saved)
        {
            Deserialize(saved);
        }

        public override string ToString()
        {
            return StaticHelpers.SerializeObject<LogResourceCount>(this, _serializedProperties, ":", ",");

        }

        public void Deserialize(string saved)
        {
            StaticHelpers.DeserializeObject<LogResourceCount>(this, saved, ":", ",");
        }

    }


    /// <summary>
    ///     this class can generically save an property name as a string
    ///     use StaticHelpers.SetKeyValue<>() to set the property when 
    ///     undoing or replaying the log
    /// </summary>
    public class LogPropertyChanged
    {
        public string OldVal { get; set; } = "";
        public string NewVal { get; set; } = "";
        public string PropertyName { get; set; } = "";

        private readonly string[] _serializedProperties = new string[] { "PropertyName", "OldVal", "NewVal" };

        public LogPropertyChanged(string name, string oldVal, string newVal)
        {
            OldVal = oldVal;
            NewVal = newVal;
            PropertyName = name;
        }

        public LogPropertyChanged(string saved)
        {
            Deserialize(saved);
        }

        public override string ToString()
        {
            return StaticHelpers.SerializeObject<LogPropertyChanged>(this, _serializedProperties, ":", ",");

        }

        public void Deserialize(string saved)
        {
            StaticHelpers.DeserializeObject<LogPropertyChanged>(this, saved, ":", ",");
        }


    }

    internal class LogRoadTrackingChanged
    {
        public string OldState { get; set; } = "";
        public string NewState { get; set; } = "";

        private readonly string[] _serializedProperties = new string[] { "OldState", "NewState" };

        public LogRoadTrackingChanged(string oldState, string newState)
        {
            this.OldState = oldState;
            this.NewState = newState;
        }

        public LogRoadTrackingChanged(string saved)
        {
            Deserialize(saved);
        }

        public override string ToString()
        {
            return StaticHelpers.SerializeObject<LogRoadTrackingChanged>(this, _serializedProperties, ":", "-");

        }

        public void Deserialize(string saved)
        {
            StaticHelpers.DeserializeObject<LogRoadTrackingChanged>(this, saved, ":", "-");
        }
    }

    public class LogList<T> : List<T>
    {

        public static LogList<T> CreateAndParse(string s)
        {
            LogList<T> obj = new LogList<T>();
            obj.Deserialize(s);
            return obj;
        }

        public override string ToString()
        {
            return Serialize();
        }

        public string Serialize()
        {
            return StaticHelpers.SerializeList<T>(this);
        }

        public void Deserialize(string s)
        {
            this.Clear();
            this.AddRange(StaticHelpers.DeserializeList<T>(s));
        }
    }

    public class LogBaronOrPirate
    {

        public TargetWeapon TargetWeapon { get; set; }          // how I targeted
        public CatanAction Action { get; set; }

        public PlayerModel SourcePlayer { get; set; } = null;
        public PlayerModel TargetPlayer { get; set; } = null;
        public int SourcePlayerIndex { get; set; } = -1;
        public int TargetPlayerIndex { get; set; } = -1;


        public int StartTileIndex { get; set; } = -1;    // where I came from (by Index0
        public TileCtrl StartTile { get; set; } = null;          // wher I came from (if I don't have to marshal

        public TileCtrl TargetTile { get; set; } = null;    // where did I move to?
        public int TargetTileIndex { get; set; } = -1; // in case I have to mashal it

        public int GameIndex { get; set; } = -1;

        private readonly string[] _serializedProperties = new string[] { "TargetWeapon", "SourcePlayerIndex", "TargetPlayerIndex", "StartTileIndex", "TargetTileIndex", "GameIndex", "Action" };

        public LogBaronOrPirate(string serialized, ILogParserHelper parseHelper)
        {
            Deserialize(serialized);
            StartTile = parseHelper.GetTile(StartTileIndex, GameIndex);
            TargetTile = parseHelper.GetTile(TargetTileIndex, GameIndex);
            SourcePlayer = parseHelper.GetPlayerData(SourcePlayerIndex);
            if (TargetPlayerIndex != -1)
            {
                TargetPlayer = parseHelper.GetPlayerData(TargetPlayerIndex);
            }

        }
        public LogBaronOrPirate(int gameIndex, PlayerModel targetPlayer, PlayerModel sourcePlayer, TileCtrl startTile, TileCtrl targetTile, TargetWeapon weapon, CatanAction action)
        {
            GameIndex = gameIndex;
            StartTile = startTile;
            Action = action;
            if (startTile != null)
            {
                StartTileIndex = startTile.Index;
            }
            TargetTile = targetTile;
            TargetTileIndex = targetTile.Index;
            TargetWeapon = weapon;
            TargetPlayer = targetPlayer;
            if (targetPlayer != null)
            {
                TargetPlayerIndex = targetPlayer.AllPlayerIndex;
            }

            SourcePlayer = sourcePlayer;
            if (sourcePlayer != null)
            {
                SourcePlayerIndex = sourcePlayer.AllPlayerIndex;
            }
        }

        public override string ToString()
        {
            return Serialize();
        }

        public string Serialize()
        {
            return StaticHelpers.SerializeObject<LogBaronOrPirate>(this, _serializedProperties, ":", ",");
        }

        public void Deserialize(string s)
        {
            StaticHelpers.DeserializeObject<LogBaronOrPirate>(this, s, ":", ",");
        }
    }

    public class LogRoadUpdate
    {
        public RoadState OldRoadState { get; set; } = RoadState.Unowned;
        public RoadState NewRoadState { get; set; } = RoadState.Unowned;
        public int Index { get; set; } = -1;
        public RoadCtrl Road { get; set; } = null;
        public int GameIndex { get; set; } = -1;
        private readonly string[] _serializedProperties = new string[] { "OldRoadState", "Index", "NewRoadState", "GameIndex" };

        public LogRoadUpdate(string s, ILogParserHelper parseHelper)
        {
            Deserialize(s);
            Road = parseHelper.GetRoad(Index, GameIndex);
        }
        public LogRoadUpdate(int gameIndex, RoadCtrl road, RoadState oldState, RoadState newState)
        {
            OldRoadState = oldState;
            Road = road;
            Index = road.Index;
            NewRoadState = newState;
            GameIndex = gameIndex;
        }
        public override string ToString()
        {
            return Serialize();
        }

        public string Serialize()
        {
            return StaticHelpers.SerializeObject<LogRoadUpdate>(this, _serializedProperties, ":", ",");
        }

        public void Deserialize(string s)
        {
            StaticHelpers.DeserializeObject<LogRoadUpdate>(this, s, ":", ",");
        }
    }

    public class LogBuildingUpdate
    {
        public BuildingCtrl Building { get; set; } = null;
        public TileCtrl Tile { get; set; } = null;

        public BuildingState OldBuildingState { get; set; } = BuildingState.None;
        public BuildingState NewBuildingState { get; set; } = BuildingState.None;
        public int TileIndex { get; set; } = -1;
        public int BuildingIndex { get; set; } = -1;
        public int GameIndex { get; set; } = -1;


        private readonly string[] _serializedProperties = new string[] { "OldBuildingState", "NewBuildingState", "BuildingIndex", "TileIndex", "GameIndex" };

        public LogBuildingUpdate(string s, ILogParserHelper parseHelper)
        {
            Deserialize(s);
            Building = parseHelper.GetBuilding(BuildingIndex, GameIndex);
            if (TileIndex != -1)
            {
                Tile = parseHelper.GetTile(TileIndex, GameIndex);
            }
        }

        public LogBuildingUpdate(int gameIndex, TileCtrl tileCtrl, BuildingCtrl buildingCtrl, BuildingState oldState, BuildingState newState)
        {
            GameIndex = gameIndex;
            Tile = tileCtrl;
            Building = buildingCtrl;
            OldBuildingState = oldState;
            BuildingIndex = buildingCtrl.Index;
            NewBuildingState = newState;
            if (tileCtrl != null)
            {
                TileIndex = tileCtrl.Index;
            }
        }
        public override string ToString()
        {
            return Serialize();
        }

        public string Serialize()
        {
            return StaticHelpers.SerializeObject<LogBuildingUpdate>(this, _serializedProperties, ":", ",");
        }

        public void Deserialize(string s)
        {
            StaticHelpers.DeserializeObject<LogBuildingUpdate>(this, s, ":", ",");
        }
    }
}