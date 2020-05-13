﻿using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System.Diagnostics;

namespace Catan10
{
    class Stacks : INotifyPropertyChanged
    {
        private readonly List<LogHeader> ActionStack = new List<LogHeader>();
        private readonly List<LogHeader> UndoStack = new List<LogHeader>();
        public event PropertyChangedEventHandler PropertyChanged;
        public Stacks() { }
        public bool CanUndo => UndoStack.Count > 0;
        public int ActionCount => ActionStack.Count;
        public void PushAction(LogHeader logHeader)
        {

            try
            {
                if (logHeader.LogType != LogType.Replay)
                {
                    UndoStack.Clear();
                }
                if (logHeader.CanUndo)
                {
                    ActionStack.Add(logHeader);
                }
                else
                {
                    ActionStack.Insert(0, logHeader);
                }


            }
            finally
            {
                NotifyPropertyChanged("GameState");
                NotifyPropertyChanged("RedoPossible"); // because it is no longer possible
                NotifyPropertyChanged("ActionStack");
                // PrintLog();

            }
        }
        public void InsertAction(LogHeader logHeader)
        {
            ActionStack.Insert(0, logHeader);
        }
        public LogHeader PeekAction
        {
            get
            {
                if (ActionStack.Count > 0) return ActionStack.Last();

                return null;
            }
        }
        public LogHeader PeekUndo
        {
            get
            {
                if (UndoStack.Count > 0) return UndoStack.Last();

                return null;
            }
        }
        public LogHeader PopAction()
        {
            if (ActionStack.Count == 0) return null;

            LogHeader lh = ActionStack.Last();
            ActionStack.Remove(lh);
            NotifyPropertyChanged("GameState");
            NotifyPropertyChanged("ActionStack");
            return lh;
        }
        public LogHeader PopUndo()
        {
            if (UndoStack.Count == 0) return null;
            var lh = UndoStack.Last();
            UndoStack.Remove(lh);
            NotifyPropertyChanged("RedoPossible");
            return lh;
        }
        public void ClearUndo()
        {
            UndoStack.Clear();
        }
        public void PushUndo(LogHeader lh)
        {
            UndoStack.Add(lh);
            NotifyPropertyChanged("RedoPossible");

        }
        //
        //  whenever we push or pop from the stack we should notify up  
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        internal void PrintLog([CallerMemberName] string caller = "")
        {
            string s = "";
            ActionStack.ForEach((lh) => s += $"{lh.Action},");
            this.TraceMessage($"{caller}: {s}");
            s = "";
            UndoStack.ForEach((lh) => s += $"{lh.Action},");
            this.TraceMessage($"{caller}: {s}");
        }
    }


    public class NewLog : INotifyPropertyChanged, IDisposable
    {

        private readonly Stacks Stacks = new Stacks();

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangedEventHandler LogChanged;

        private List<CatanMessage> MessageLog { get; } = new List<CatanMessage>();
        public MainPage Page { get; internal set; }
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

        public NewLog()
        {
            Page = MainPage.Current;
            Stacks.PropertyChanged += Stacks_PropertyChanged;
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

        public Task PushAction(LogHeader logHeader)
        {
            GameState currentState = GameState;
            try
            {
                if (logHeader.LogType != LogType.Replay)
                {
                    Stacks.ClearUndo();
                }
                if (logHeader.CanUndo)
                {
                    Stacks.PushAction(logHeader);
                }
                else
                {
                    Stacks.InsertAction(logHeader);
                }

                if (logHeader.LocallyCreated)
                {
                    CatanMessage message = new CatanMessage()
                    {
                        Data = logHeader,
                        Origin = Page.TheHuman.PlayerName

                    };

                    return PostLogMessage(message);
                }

                return Task.CompletedTask;
            }
            finally
            {
                NotifyPropertyChanged("GameState");
                // PrintLog();
            }
        }


        //
        //  whenever we push or pop from the stack we should 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            LogChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool CanRedo => Stacks.CanUndo;

        public LogHeader PeekAction => Stacks.PeekAction;
        public LogHeader PeekUndo => Stacks.PeekUndo;

        private Task PostLogMessage(CatanMessage message)
        {

            var serviceData = Page.MainPageModel.ServiceData;
            return serviceData.Proxy.PostLogMessage(serviceData.SessionInfo.Id, message);
        }


        /// <summary>
        ///     Redo is called in two cases -- if the user clicks Redo from the UI or if a message comes from another machine
        /// </summary>
        /// <returns></returns>
        public Task Redo(CatanMessage message = null)
        {
            try
            {
                var logHeader = Stacks.PopUndo();
                Contract.Assert(logHeader.LogType == LogType.Undo);
                logHeader.LogType = LogType.Replay;


                if (message != null)
                {
                    //
                    //  when we get an Redo command, we treat it as if somebody clicked on Redo in the UI
                    //  but that means we rely on the Undo stack being identical -- assert it.
                    Contract.Assert(logHeader.LogId == ((LogHeader)message.Data).LogId);
                }

                ILogController logController = logHeader as ILogController;
                Contract.Assert(logController != null, "Every LogHeader is a LogController!");

                //
                //  this will push the action onto the ActionStack
                return logController.Redo(Page, logHeader);
            }
            finally
            {
                //   PrintLog();
                NotifyPropertyChanged("Do");
                NotifyPropertyChanged("Undo");
                NotifyPropertyChanged("GameState");

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
        public async Task Undo(CatanMessage message = null)
        {
            try
            {
                var undoneLogHeader = Stacks.PeekAction;
                if (!undoneLogHeader.CanUndo)
                {
                    return;
                }

                Stacks.PopAction();


                if (message != null)
                {
                    LogHeader logHeader = message.Data as LogHeader;
                    Contract.Assert(logHeader != null);
                    Contract.Assert(undoneLogHeader.LogId == logHeader.LogId);

                }
                ILogController logController = undoneLogHeader as ILogController;
                Contract.Assert(logController != null, "all log entries must also be log controllers!");
                await logController.Undo(this.Page, undoneLogHeader);
                undoneLogHeader.LogType = LogType.Undo;

                Stacks.PushUndo(undoneLogHeader);

                if (message == null)
                {
                    message = new CatanMessage()
                    {
                        Data = undoneLogHeader,
                        Origin = Page.TheHuman.PlayerName
                    };
                    await PostLogMessage(message);
                }
            }
            finally
            {
                //  PrintLog();
                NotifyPropertyChanged("GameState");

            }

        }



        internal void RecordMessage(CatanMessage message)
        {
            if (MessageLog.Count > 1)
            {
                Contract.Assert(message.Sequence - 1 == MessageLog.Last().Sequence);
            }

            MessageLog.Add(message);
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

}