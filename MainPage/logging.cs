using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace Catan10
{

    /// <summary>
    ///     I've moved to having a state machine for the log from having a Type on each logline
    ///     this means we can set the state on the log, do a bunch of actions, and then change
    ///     the log state and we don't have to flow the logType through the whole system. This is possible
    ///     because we Undo and Replay in specific places so we can set the right state there.
    ///     
    ///     the only LogType that is special then is an "override" that is LogType.DoNotLog
    /// </summary>
    public enum LogState
    {
        Normal,
        Replay,
        Undo
    }

    public class Log : List<LogEntry>
    {
        private string _saveFileName = "";
        StorageFolder _folder = null;
        StorageFile _file = null;


        public LogState State { get; set; } = LogState.Normal;
        private int _lastLogRecordWritten = 0;

        public string DisplayName
        {
            get
            {
                return File.DisplayName;
            }
        }

        public async Task Init(string fileName)
        {
            _saveFileName = fileName + MainPage.SAVED_GAME_EXTENSION;
            _folder = await StaticHelpers.GetSaveFolder();
            _file = await _folder.CreateFileAsync(_saveFileName, CreationCollisionOption.OpenIfExists);
            _lastLogRecordWritten = 0;
        }

        public Log(StorageFile file)
        {
            _file = file;
            _saveFileName = _file.DisplayName;
        }

        public Log()
        {

        }

        public StorageFile File
        {
            get
            {
                return _file;
            }
        }

        public List<LogEntry> LogEntries
        {
            get
            {
                return this;
            }
        }

        public void AppendLogLineNoDisk(LogEntry le)
        {
            if (le.LogType == LogType.DoNotLog) return;

            switch (this.State)
            {
                case LogState.Normal:
                    le.LogType = LogType.Normal;
                    break;
                case LogState.Replay:
                    return;
                case LogState.Undo:
                    le.LogType = LogType.Undo;
                    break;
                default:
                    break;
            }




            le.LogLineIndex = Count;

            if (le.LogType == LogType.Undo)
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    if (this[i].LogType != LogType.Undo)
                    {
                        if (this[i].Undone == false)
                        {
                            if (this[i].Action == le.Action)
                            {
                                le.IndexOfUndoneAction = i;
                                this[i].Undone = true;
                                break;
                            }
                            else
                            {

                               // this.TraceMessage($"seems we have an non-balanced undo.  Action={le.Action}");
                               // le.Undone = true; // undo in place
                               // continue;
                            }
                        }
                    }
                }

            }

            Add(le);
            Debug.WriteLine(le);
        }

        public async Task AppendLogLine(LogEntry le, bool save = true)
        {

            AppendLogLineNoDisk(le);
            if (save && this.State != LogState.Replay)
            {
                await WriteUnwrittenLinesToDisk();
            }

            //Debug.WriteLine(le);
        }

        /// <summary>
        ///     go backwards through the log and persist all the records that haven't been persisted yet
        ///     this will be called the first time a road or a building is changed, a roll happens, or 
        ///     a GameState changed...it is not called if Properties are set
        /// </summary>
        /// <returns></returns>
        public async Task WriteUnwrittenLinesToDisk()
        {
            try
            {


                if (this.Count == 0) return;
                int count = this.Count; // this might change while we loop because of the await
                                        
                                        
                var file = await _folder.CreateFileAsync(_saveFileName, CreationCollisionOption.OpenIfExists);
                for (int i = _lastLogRecordWritten; i < count; i++)
                {
                    LogEntry le = this[i];
                    if (!le.Persisted)
                    {
                        string s = String.Format($"{le.Serialize()}\r\n");
                        await FileIO.AppendTextAsync(file, s);
                        le.Persisted = true;
                        
                    }

                }
                _lastLogRecordWritten = count;
                
            }
            catch (Exception e)
            {
              //  this.TraceMessage($"Caught Exception when writing to disk: {e}");
                //
                //  just eat it.  we'll save it the next time 
            }


        }

        public async Task<bool> Parse(ILogParserHelper helper)
        {
            if (this.Count != 0)
                return true; // already parsed this

            string contents = await FileIO.ReadTextAsync(_file);

            string[] tokens = contents.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Count() < 5)
            {
                this.TraceMessage("Invalid Log -- too few log lines");
                return false;
            }
            foreach (string line in tokens)
            {
                LogEntry le = new LogEntry(line, helper);
                le.Persisted = true; 
                AppendLogLineNoDisk(le);

            }

            return true;

        }




    }

    public interface ILogParserHelper
    {
        TileCtrl GetTile(int tileIndex, int gameIndex);
        RoadCtrl GetRoad(int roadIndex, int gameIndex);
        BuildingCtrl GetBuilding(int buildingIndex, int gameIndex);
        PlayerData GetPlayerData(int playerIndex);
    }

    public interface ILog
    {
        Task AddLogEntry(PlayerData player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0);
        void PostLogEntry(PlayerData player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0);
    }

    public enum LogType { Normal, Undo, Replay, DoNotLog, DoNotUndo, Test };


    //   an object that encapsulates an action that has happned in the game
    public class LogEntry
    {
        public int LogLineIndex { get; set; } = -1;
        public LogType LogType { get; set; } = LogType.Normal;
        public bool Undone { get; set; } = false;   // this flag says that the user has undone this log record - on disc it is always false since we are append only and don't update rows
        public int IndexOfUndoneAction { get; set; } = -1; // if this is a LogType == LogType.Undo then this is the index of the LogEntry that was undone
        public GameState GameState { get; set; }
        public PlayerData PlayerData { get; set; }
        public string PlayerDataString { get; set; } = "";
        public CatanAction Action { get; set; }
        public int Number { get; set; } = -1;
        public bool StopProcessingUndo { get; set; } = true; // when an action takes place, does it show up in the UI?  Cononical example:  supplemental also does changes current player.  when you undo, you need to undo both records        
        public object Tag { get; set; } = null;
        private string TagAsString { get; set; } = "";
        public string MemberName { get; set; } = "";
        public int LineNumber { get; set; } = -1;
        public string Path { get; set; } = "";

        public bool Persisted { get; set; } = false;
        public string PlayerName
        {
            get
            {
                if (PlayerData == null)
                    return "<none>";

                return PlayerData.PlayerName;
            }
        }

        public LogEntry(PlayerData p, GameState s, CatanAction a, int n, bool stopUndo, LogType type = LogType.Normal, object tag = null, [CallerMemberName] string cmn = "", [CallerLineNumber] int cln = 0, [CallerFilePath] string cfp = "")
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

        static readonly private string[] _serializeProperties = new string[] { "LogLineIndex", "Persisted", "LogType", "Undone", "PlayerDataString", "GameState", "Action", "Number", "IndexOfUndoneAction", "StopProcessingUndo", "TagAsString", "MemberName", "LineNumber"}; // "Path" removed
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

            return $"Index:{this.LogLineIndex, -5}| Action:{Action, -25} | LogType:{LogType, -10} | Undone:{Undone, -6} | UndoneIndex:{IndexOfUndoneAction, 5} | {PlayerDataString, -10} | {TagAsString, -20}";


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

            if (PlayerData != null && PlayerDataString == "")
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



        public bool Undoable
        {
            get
            {
                return (Action != CatanAction.ChangedState);
            }
        }


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

        private string[] _serializedProperties = new string[] { "FirstPlayerIndex" };

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
        private string[] _serializedProperties = new string[] { "From", "To" };
        public LogChangePlayer(int old, int newIdx)
        {
            From = old;
            To = newIdx;
            //   this.TraceMessage(this.ToString());
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

        private string[] _serializedProperties = new string[] { "OldState", "NewState" };

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


        private string[] _serializedProperties = new string[] { "OldVal", "NewVal" };

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


        private string[] _serializedProperties = new string[] { "OldVal", "NewVal", "ResourceType" };

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

        private string[] _serializedProperties = new string[] { "PropertyName", "OldVal", "NewVal" };

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

        private string[] _serializedProperties = new string[] { "OldState", "NewState" };

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

        public PlayerData SourcePlayer { get; set; } = null;
        public PlayerData TargetPlayer { get; set; } = null;
        public int SourcePlayerIndex { get; set; } = -1;
        public int TargetPlayerIndex { get; set; } = -1;


        public int StartTileIndex { get; set; } = -1;    // where I came from (by Index0
        public TileCtrl StartTile { get; set; } = null;          // wher I came from (if I don't have to marshal

        public TileCtrl TargetTile { get; set; } = null;    // where did I move to?
        public int TargetTileIndex { get; set; } = -1; // in case I have to mashal it

        public int GameIndex { get; set; } = -1;

        private string[] _serializedProperties = new string[] { "TargetWeapon", "SourcePlayerIndex", "TargetPlayerIndex", "StartTileIndex", "TargetTileIndex", "GameIndex", "Action" };

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
        public LogBaronOrPirate(int gameIndex, PlayerData targetPlayer, PlayerData sourcePlayer, TileCtrl startTile, TileCtrl targetTile, TargetWeapon weapon, CatanAction action)
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
        private string[] _serializedProperties = new string[] { "OldRoadState", "Index", "NewRoadState", "GameIndex" };

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


        private string[] _serializedProperties = new string[] { "OldBuildingState", "NewBuildingState", "BuildingIndex", "TileIndex", "GameIndex" };

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
                TileIndex = tileCtrl.Index;
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