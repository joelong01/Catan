using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.Storage;

namespace Catan10
{
    internal class Stacks : INotifyPropertyChanged
    {
        private readonly Stack<LogHeader> DoneStack = new Stack<LogHeader>();
        private readonly Queue<LogHeader> PendingQueue = new Queue<LogHeader>();
        private readonly Stack<LogHeader> UndoneStack = new Stack<LogHeader>();
        public Stacks()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int ActionCount => DoneStack.Count;
        public bool CanRedo => UndoneStack.Count > 0;
        public bool CanUndo
        {
            get
            {
                if (DoneStack.Count == 0) return false;

                return DoneStack.Peek().CanUndo;
            }
        }

        public LogHeader PeekAction
        {
            get
            {
                if (DoneStack.Count > 0) return DoneStack.Peek();

                return null;
            }
        }

        public LogHeader PeekUndo
        {
            get
            {
                if (UndoneStack.Count > 0) return UndoneStack.Peek();

                return null;
            }
        }


        //
        //  whenever we push or pop from the stack we should notify up
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void PrintLog([CallerMemberName] string caller = "")
        {
            int lines = 0;
            string actionLine = $"[CallerFilePath={caller}][Actions={DoneStack.Count}]";
            string undoLine = $"[CallerFilePath={caller}][Undo={UndoneStack.Count}]";

            foreach (var lh in DoneStack)
            {

                actionLine += $"[{lh.Action} - {lh.LogId.ToString().Substring(0, 6)}],";
                lines++;
                if (lines == 3) break;

            }
            foreach (var lh in UndoneStack)
            {
                undoLine += $"[{lh.Action} - {lh.LogId.ToString().Substring(0, 6)}],";
                lines++;
                if (lines == 3) break;

            }


            Debug.WriteLine(actionLine);
            Debug.WriteLine(undoLine + "\n");
        }

        public void ClearUndo()
        {
            UndoneStack.Clear();
        }

        public LogHeader PopAction()
        {
            if (DoneStack.Count == 0) return null;

            LogHeader lh = DoneStack.Pop();
            NotifyPropertyChanged("GameState");
         //   NotifyPropertyChanged("ActionStack");
            return lh;
        }

        public LogHeader PopUndo()
        {
            if (UndoneStack.Count == 0) return null;
            var lh = UndoneStack.Pop();
            NotifyPropertyChanged("RedoPossible");
            return lh;
        }

        public void PushAction(LogHeader logHeader)
        {
            try
            {

                if (logHeader.LogType != LogType.Redo)
                {
                    UndoneStack.Clear();
                }

                DoneStack.Push(logHeader);

            }
            finally
            {
                NotifyPropertyChanged("GameState");
             //   NotifyPropertyChanged("RedoPossible"); // because it is no longer possible
             //   NotifyPropertyChanged("ActionStack");
                PrintLog();
            }
        }

        public void PushUndo(LogHeader lh)
        {
            UndoneStack.Push(lh);
            NotifyPropertyChanged("RedoPossible");
            PrintLog();
        }
    }

    public class NewLog : INotifyPropertyChanged, IDisposable
    {
        private readonly Stacks Stacks = new Stacks();

        private bool _writing = false;

        public NewLog()
        {
            Stacks.PropertyChanged += Stacks_PropertyChanged;
            DateTime dt = DateTime.Now;

            string ampm = dt.TimeOfDay.TotalMinutes > 720 ? "PM" : "AM";
            string min = dt.TimeOfDay.Minutes.ToString().PadLeft(2, '0');

            SaveFileName = String.Format($"{dt.TimeOfDay.Hours % 12}.{min} {ampm}{MainPage.SAVED_GAME_EXTENSION}");
        }

        public event PropertyChangedEventHandler LogChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private ConcurrentQueue<CatanMessage> MessageLog { get; } = new ConcurrentQueue<CatanMessage>();
        private string SaveFileName { get; set; }
        public bool CanRedo => Stacks.CanRedo;
        public bool CanUndo => Stacks.CanUndo;

        public IGameController GameController => MainPage.Current;

        public GameState GameState
        {
            get
            {
                if (Stacks.ActionCount == 0)
                {
                    return GameState.WaitingForNewGame;
                }

                return Stacks.PeekAction.NewState;
            }
        }
        public LogHeader PeekAction => Stacks.PeekAction;
        public LogHeader PeekUndo => Stacks.PeekUndo;
        //
        //  whenever we push or pop from the stack we should
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            LogChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void SaveLogAsync()
        {
            await Task.Run(() =>
            {
                WriteToDisk();
            });
        }

        /// <summary>
        ///     forward the event to the Log's customers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stacks_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(e.PropertyName);
        }

        private void WriteToDisk()
        {
            try
            {
                if (_writing) return;
                _writing = true;

                string json = CatanProxy.Serialize(MessageLog, true);
                var folder = MainPage.Current.SaveFolder;
                var file = folder.CreateFileAsync(SaveFileName, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                FileIO.WriteTextAsync(file, json).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (System.IO.IOException)
            {
                // eat it
            }
            catch (Exception e)
            {
                this.TraceMessage($"{e}");
            }
            finally
            {
                _writing = false;
            }
        }

        internal void RecordMessage(CatanMessage message)
        {
            if (MessageLog.Count > 1)
            {
                Contract.Assert(message.Sequence - 1 == MessageLog.Last().Sequence);
            }

            MessageLog.Enqueue(message);
            SaveLogAsync();
        }

        public void Dispose()
        {
        }

        public void DumpLogRecords()
        {
            MessageLog.ForEach((m) =>
            {
                LogHeader logHeader = (LogHeader)m.Data;

                this.TraceMessage($"Messages: [Sequence={m.Sequence}]\t[id={logHeader.LogId}][Origin={m.Origin}]\t" +
                                  $"[Action={logHeader.Action}]\tPlayer=[{logHeader.PlayerName}]\t[LogType={logHeader.LogType}]");
            });
        }

        public Task PushAction(LogHeader logHeader)
        {
            try
            {
                if (logHeader.LogType == LogType.Normal)
                {
                    Stacks.ClearUndo();
                }
                logHeader.LogType = LogType.Normal;
                Stacks.PushAction(logHeader);
                return Task.CompletedTask;
            }
            finally
            {
                // PrintLog();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Task Redo(LogHeader incomingLogHeader)
        {
            try
            {
                var logHeader = Stacks.PopUndo();
                Contract.Assert(logHeader.LogType == LogType.Undo);
                Contract.Assert(logHeader.LogId == incomingLogHeader.LogId);
                logHeader.LogType = LogType.Redo;  // 5/21/2020:  We need this so that we know to know clear the undo stack on the push
                Stacks.PushAction(logHeader);          
                return Task.CompletedTask;
            }
            finally
            {
                GameController.CompleteRedo();
                //   PrintLog();
            }
        }

        /// <summary>
        ///     this is an Undo initiated by the Game UI (e.g. a player clicks on "Undo") or by recieving an Undo message from the service
        ///     because some other player clicked on Undo
        ///
        ///     pops the action off the Undo stack and calls Undo
        ///
        ///     CanUndo actions were added to the end of the list and non-undoable are added to the beginning
        ///
        ///     if a message is passed in it means that the call originated from the service. make sure that the message is the same as the one on the stack
        ///     if no message is passed in, call the service with an undo message
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task Undo(LogHeader incomingLogHeader)
        {
            try
            {
                var logHeader = Stacks.PeekAction;
                Contract.Assert(logHeader.LogId == incomingLogHeader.LogId);
                if (!logHeader.CanUndo)
                {
                    return Task.CompletedTask;
                }
                Stacks.PopAction();
                logHeader.LogType = LogType.Undo;
                Stacks.PushUndo(logHeader);                
                return Task.CompletedTask;
            }
            finally
            {
                GameController.CompleteUndo();
                //  PrintLog();
            }
        }
    }
}