using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Catan10
{
    public class NewLog
    {
        private readonly List<LogHeader> ActionStack = new List<LogHeader>();
        private readonly List<LogHeader> UndoStack = new List<LogHeader>();
        private List<CatanMessage> MessageLog { get; } = new List<CatanMessage>();
        public MainPage Page { get; internal set; }
        public GameState GameState
        {
            get
            {

                if (ActionStack.Count == 0)
                {
                    return GameState.WaitingForNewGame;
                }

                return ActionStack.Last().NewState;

            }

        }

        public NewLog(MainPage p)
        {
            Page = p;
        }

        public Task PushAction(LogHeader logHeader)
        {
            try
            {
                if (logHeader.CanUndo)
                {
                    ActionStack.Add(logHeader);
                }
                else
                {
                    ActionStack.Insert(0, logHeader);
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
                PrintLog();

            }
        }

        public LogHeader PeekAction => ActionStack.Last();
        public LogHeader PeekUndo => UndoStack.Last();
        public bool CanRedo => UndoStack.Count > 0;


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
                var logHeader = UndoStack.Last();
                UndoStack.RemoveAt(UndoStack.Count - 1);
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
                PrintLog();

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
                var undoneLogHeader = ActionStack.Last();
                if (!undoneLogHeader.CanUndo)
                {
                    return;
                }

                ActionStack.RemoveAt(ActionStack.Count - 1);

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

                UndoStack.Push(undoneLogHeader);

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
                PrintLog();

            }

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

        internal void RecordMessage(CatanMessage message)
        {
            if (MessageLog.Count > 1)
            {
                Contract.Assert(message.Sequence - 1 == MessageLog.Last().Sequence);
            }

            MessageLog.Add(message);
            MessageLog.ForEach((m) =>
            {
                LogHeader logHeader = (LogHeader)m.Data;

                this.TraceMessage($"Messages: [Sequence={m.Sequence}]\t[id={logHeader.LogId}][Origin={m.Origin}]\t" +
                                  $"[Action={logHeader.Action}]\tPlayer=[{logHeader.PlayerName}]\t[LogType={logHeader.LogType}]");
            });            
    }
}
}