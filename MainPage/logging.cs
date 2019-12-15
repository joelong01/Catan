using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

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

    public delegate void RedoPossibleHandler(bool redo);

    /// <summary>
    ///     A log has two stacks:  an action stack and an undo stack.
    ///     1. when an Undo command happens, it is popped from the Action stack, the action is Undone, and it is pushed to the Undo Stack
    ///     2. when a Redo is done, it is popped from the Undone stack, played forward, and then pushed on the Action stack
    ///     3. when something is added to the Action stack and it is *not* part of a Replay, the Undo stack is cleared.
    /// </summary>

    public class Log : IDisposable
    {
        public RedoPossibleHandler OnRedoPossible;
        private string _saveFileName = "";
        private StorageFolder _folder = null;
        IRandomAccessStream _randomAccessStream = default;

        private StorageFile _file = null;

        public LogState State { get; set; } = LogState.Normal;

        public string DisplayName => File.DisplayName;

        public async Task Init(string fileName)
        {
            _saveFileName = fileName + MainPage.SAVED_GAME_EXTENSION;
            _folder = await StaticHelpers.GetSaveFolder();
            _file = await _folder.CreateFileAsync(_saveFileName, CreationCollisionOption.OpenIfExists);
            _randomAccessStream = await _file.OpenAsync(FileAccessMode.ReadWrite);

        }

        public void Dispose()
        {
            _randomAccessStream?.Dispose();
        }




        public Log(StorageFile file)
        {
            _file = file;
            _saveFileName = _file.DisplayName;
            _folder = StaticHelpers.GetSaveFolder().Result;

        }

        public Log()
        {

        }

        public StorageFile File => _file;

        private readonly List<LogEntry> ActionStack  = new List<LogEntry>();
        private readonly List<LogEntry> UndoStack  = new List<LogEntry>();

        public IReadOnlyCollection<LogEntry> Actions => ActionStack;

        public LogEntry PopAction()
        {
            if (ActionStack.Count == 0) return null;
            LogEntry le = ActionStack.Last();
            ActionStack.Remove(le);
            return le;
        }

        public void PushAction(LogEntry le)
        {
            UndoStack.Clear();
            ActionStack.Add(le);
            NotifyRedoPossible();
        }

        public void PushUndo(LogEntry le)
        {
            UndoStack.Add(le);
            NotifyRedoPossible();
        }
        private void NotifyRedoPossible()
        {
          OnRedoPossible?.Invoke(UndoStack.Count > 0);
        }
        public LogEntry PopUndo()
        {
            if (UndoStack.Count == 0) return null;
            LogEntry le = UndoStack.Last();
            UndoStack.Remove(le);
            NotifyRedoPossible();
            return le;
        }

        public LogEntry Last()
        {
            if (ActionStack.Count > 0)
            {
                return ActionStack.Last();
            }

            return null;
        }

        public int ActionCount => ActionStack.Count;
        public int UndoCount => UndoStack.Count;

        public GameState GameState => ActionStack.Last().GameState;



        public void AppendLogLineNoDisk(LogEntry le)
        {
            if (le.LogType == LogType.DoNotLog || le.LogType == LogType.Undo)
            {
                return;
            }

            switch (this.State)
            {
                case LogState.Normal:
                    le.LogType = LogType.Normal;
                    UndoStack.Clear();
                    NotifyRedoPossible();
                    break;
                case LogState.Replay:
                    le.LogType = LogType.Replay;
                    break;
                case LogState.Undo:
                    //
                    //  don't log anything on Undo -- push to the undo stack
                    return;
                default:
                    break;
            }


            ActionStack.Add(le);

        }

        public async Task AppendLogLine(LogEntry le, bool save = true)
        {

            AppendLogLineNoDisk(le);
            if (save && this.State != LogState.Replay)
            {
                await WriteLogToDisk();
            }

            //Debug.WriteLine(le);
        }

        //
        //  we have to write the whole thing because we might have undo some records and so they 
        //  need to be thrown away.
        public async Task WriteLogToDisk()
        {
            if (ActionStack.Count == 0) return;




            _randomAccessStream.Size = 0;
            using (var outputStream = _randomAccessStream.GetOutputStreamAt(0))
            {
                using (var dataWriter = new DataWriter(outputStream))
                {
                    foreach (var le in ActionStack)
                    {
                        string s = String.Format($"{le.Serialize()}\r\n");
                        dataWriter.WriteString(s);

                    }

                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();
                }
            }


        }




        //public async Task<bool> Parse(ILogParserHelper helper)
        //{
        //    if (this.Count != 0)
        //    {
        //        return true; // already parsed this
        //    }

        //    string contents = await FileIO.ReadTextAsync(_file);

        //    string[] tokens = contents.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        //    if (tokens.Count() < 5)
        //    {
        //        this.TraceMessage("Invalid Log -- too few log lines");
        //        return false;
        //    }
        //    foreach (string line in tokens)
        //    {
        //        LogEntry le = new LogEntry(line, helper)
        //        {
        //            Persisted = true
        //        };

        //        Add(le);
        //        Debug.WriteLine(le);
        //    }

        //    return true;

        //}

        internal void Reset()
        {
            //base.Clear();
            //this.State = LogState.Normal;
        }

        internal void Start()
        {
            //base.Clear();
            //this.State = LogState.Normal;
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

    public enum LogType { Normal, Undo, Replay, DoNotLog, DoNotUndo };


    //   an object that encapsulates an action that has happned in the game
    public class LogEntry
    {
        public LogType LogType { get; set; } = LogType.Normal;

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
                case CatanAction.TotalGoldChanged:
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
        private readonly string[] _serializedProperties = new string[] { "From", "To", "OldRandomGoldTiles", "NewRandomGoldTiles" };
        public LogChangePlayer(int old, int newIdx, List<int> rgtOld, List<int> rgtNew)
        {
            From = old;
            To = newIdx;
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

        public PlayerData SourcePlayer { get; set; } = null;
        public PlayerData TargetPlayer { get; set; } = null;
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