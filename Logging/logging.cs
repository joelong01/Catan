using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

    ///     A log has two stacks:  an action stack and an undo stack.
    ///     1. when an Undo command happens, it is popped from the Action stack, the action is Undone, and it is pushed to the Undo Stack
    ///     2. when a Redo is done, it is popped from the Undone stack, played forward, and then pushed on the Action stack
    ///     3. when something is added to the Action stack and it is *not* part of a Replay, the Undo stack is cleared.
    /// </summary>

    public class Log : IDisposable, INotifyPropertyChanged
    {

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

        private readonly List<LogEntry> ActionStack = new List<LogEntry>();
        private readonly List<LogEntry> UndoStack = new List<LogEntry>();

        public event PropertyChangedEventHandler PropertyChanged;

        public IReadOnlyCollection<LogEntry> Actions => ActionStack;

        public LogEntry PopAction()
        {
            if (ActionStack.Count == 0) return null;

            LogEntry le = ActionStack.Last();
            ActionStack.Remove(le);
            NotifyPropertyChanged("GameState");
            return le;
        }

        public void PushAction(LogEntry le)
        {
            if (this.State != LogState.Replay)
            {
                UndoStack.Clear();
            }


            ActionStack.Add(le);
            NotifyPropertyChanged("GameState");
            NotifyPropertyChanged("RedoPossible");
        }

        public void PushUndo(LogEntry le)
        {
            UndoStack.Add(le);
            NotifyPropertyChanged("RedoPossible");

        }

        public bool RedoPossible
        {
            get
            {
                return UndoStack.Count > 0;
            }

        }
        public LogEntry PopUndo()
        {
            if (UndoStack.Count == 0) return null;
            LogEntry le = UndoStack.Last();
            UndoStack.Remove(le);
            NotifyPropertyChanged("RedoPossible");
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

        public GameState GameState
        {
            get
            {
                if (ActionStack.Count == 0) return GameState.WaitingForNewGame;

                return ActionStack.Last().GameState;
            }
        }



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
                    NotifyPropertyChanged("RedoPossible");
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


            this.PushAction(le);

        }

        public async Task AppendLogLine(LogEntry le, bool save = true)
        {

            AppendLogLineNoDisk(le);
            if (save && this.State != LogState.Replay)
            {
                await WriteFullLogToDisk();
            }

            //Debug.WriteLine(le);
        }

        //
        //  we have to write the whole thing because we might have undo some records and so they 
        //  need to be thrown away.
        public async Task WriteFullLogToDisk()
        {
            if (ActionStack.Count == 0) return;

            StringBuilder sb = new StringBuilder();
            foreach (var le in ActionStack)
            {
                sb.Append($"{le.Serialize()}\r\n");
            }


            _randomAccessStream.Size = 0;
            using (var outputStream = _randomAccessStream.GetOutputStreamAt(0))
            {
                using (var dataWriter = new DataWriter(outputStream))
                {
                    dataWriter.WriteString(sb.ToString());
                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();
                }
            }


        }

        public async Task AppendLogEntry(LogEntry le)
        {
            using (var outputStream = _randomAccessStream.GetOutputStreamAt(_randomAccessStream.Size))
            {
                using (var dataWriter = new DataWriter(outputStream))
                {
                    dataWriter.WriteString($"{le.Serialize()}\r\n");
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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}