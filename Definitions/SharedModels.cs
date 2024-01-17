using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catan10
{
    public enum ActionType { Normal, Undo, Redo, Replay, Retry };
    public enum MessageDirection { ClientToServer, ServerToClient};

    public enum CatanError
    {
        DevCardsSoldOut,
        NoMoreResource,
        LimitExceeded,
        NoGameWithThatName,
        NoPlayerWithThatName,
        NotEnoughResourcesToPurchase,
        MissingData,
        BadTradeResources,
        NoResource,
        BadEntitlement,
        BadParameter,
        BadLogRecord,
        PlayerAlreadyRegistered,
        GameAlreadStarted,
        Unknown,
        InsufficientResource,
        Unexpected,
        NoError,
    }

    public enum MessageType { UpdateCompanion, BroadcastMessage, PrivateMessage, CreateGame, DeleteGame, JoinGame, RejoinGame, LeaveGame, Ack };

    /// <summary>
    ///  This is the class that we send to the service to synchronize state.
    ///  it is Deserialized in the service.
    ///
    ///  Data is a LogHeader of some type
    ///  DataTypeName is the name of the derived LogHeader type
    ///  Sequence is set by the service and is the order of the log it has received
    ///  Origin is the PlayerName that created the message
    ///  CatanMessageType is the obvious
    ///
    /// </summary>
    public class CatanMessage
    {
        #region Delegates + Fields + Events + Enums

        private object _data;

        #endregion Delegates + Fields + Events + Enums

        #region Properties
        public int Sequence { get; set; } = 0;
        public Guid MessageId { get; set; } = Guid.NewGuid();
        
        public ActionType ActionType { get; set; } = ActionType.Normal;
        public MessageType MessageType { get; set; }
        public MessageDirection MessageDirection { get; set; } = MessageDirection.ClientToServer;

        public string From { get; set; } = "";
        
        public string To { get; set; } = "*";
        public string DataTypeName { get; set; } = "";

        public GameInfo GameInfo { get; set; } = null;

        public object Data
        {
            get => _data;
            set
            {
                _data = value;
                
            }
        }

        

        #endregion Properties

        #region Constructors + Destructors

        public CatanMessage()
        {
        }

        #endregion Constructors + Destructors

        #region Methods

        public override string ToString()
        {
            return $"[Type={MessageType}][Sequence={Sequence}][Origin={From}][DataType={DataTypeName}]";
        }

        #endregion Methods
    }

    public class CatanRequest
    {
        #region Properties

        public object Body { get; set; } = null;
        public string Url { get; set; } = "";

        #endregion Properties

        #region Constructors + Destructors

        public CatanRequest()
        {
        }

        public CatanRequest(string u, object b)
        {
            Url = u; Body = b;
        }

        #endregion Constructors + Destructors

        #region Methods

        public override string ToString()
        {
            return $"[Url={Url}][Body={Body?.ToString()}]";
        }

        #endregion Methods
    }

    public class CatanResult
    {
        #region Delegates + Fields + Events + Enums

        private CatanRequest _request = new CatanRequest();
        private string request;

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public CatanRequest CantanRequest
        {
            get
            {
                return _request;
            }
            set
            {
                if (value != _request)
                {
                    _request = value;
                }
            }
        }

        public string Description { get; set; }
        public CatanError Error { get; set; } = CatanError.Unknown;
        public List<KeyValuePair<string, object>> ExtendedInformation { get; } = new List<KeyValuePair<string, object>>();
        public string FilePath { get; set; }
        public string FunctionName { get; set; }
        public Guid ID { get; set; } = Guid.NewGuid();
        public int LineNumber { get; set; }
        public string Request { get => _request.Url; set => request = value; }
        public DateTime Time { get; set; } = DateTime.Now;
        // this gives us an ID at creation time that survives serialization and is globally unique
        public string Version { get; set; } = "2.0";

        #endregion Properties

        #region Constructors + Destructors

        public CatanResult() // for the Serializer
        {
        }

        public CatanResult(CatanError error, [CallerMemberName] string fName = "", [CallerFilePath] string codeFile = "", [CallerLineNumber] int lineNumber = -1)
        {
            Error = error;
            FunctionName = fName;
            FilePath = codeFile;
            LineNumber = lineNumber;
        }

        #endregion Constructors + Destructors

        #region Methods

        public static bool operator !=(CatanResult a, CatanResult b)
        {
            return !(a == b);
        }

        public static bool operator ==(CatanResult a, CatanResult b)
        {
            if (a is null || b is null)
            {
                if (b is null && a is null)
                {
                    return true;
                }

                return false;
            }

            if (a.ExtendedInformation?.Count != b.ExtendedInformation?.Count)
            {
                return false;
            }
            if (a.ExtendedInformation != null)
            {
                if (b.ExtendedInformation == null) return false;
                for (int i = 0; i < a.ExtendedInformation?.Count; i++)
                {
                    if (a.ExtendedInformation[i].Key != b.ExtendedInformation[i].Key)
                    {
                        return false;
                    }

                    //
                    //  going to ignore the value for now
                }
            }

            return
                (
                    a.Description == b.Description &&
                    a.FunctionName == b.FunctionName &&
                    a.FilePath == b.FilePath &&
                    a.LineNumber == b.LineNumber &&
                    a.Request == b.Request &&
                    a.Error == b.Error
                 );
        }

        public override bool Equals(object obj)
        {
            return (CatanResult)obj == this;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 97 + Description.GetHashCode();
            hash = hash * 97 + FunctionName.GetHashCode();
            hash = hash * 97 + FilePath.GetHashCode();
            hash = hash * 97 + LineNumber.GetHashCode();
            hash = hash * 97 + Request.GetHashCode();
            hash = hash * 97 + Error.GetHashCode();
            return hash;
        }

        #endregion Methods
    }

    public class CatanServiceMessage
    {
        #region Properties

        public GameInfo GameInfo { get; set; }
        public string PlayerName { get; set; }

        #endregion Properties
    }

    public class HouseRules : INotifyPropertyChanged
    {
        bool _beforeFirstInvasion = true;
        bool _wallProtectsCity = true;
        bool _moveBaronBeforeRoll = true;
        public bool MoveBaronBeforeRoll
        {
            get
            {
                return _moveBaronBeforeRoll;
            }
            set
            {
                if (_moveBaronBeforeRoll != value)
                {
                    _moveBaronBeforeRoll = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool WallProtectsCity
        {
            get
            {
                return _wallProtectsCity;
            }
            set
            {
                if (_wallProtectsCity != value)
                {
                    _wallProtectsCity = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool HideBaronBeforeFirstInvasion
        {
            get
            {
                return _beforeFirstInvasion;
            }
            set
            {
                if (_beforeFirstInvasion != value)
                {
                    _beforeFirstInvasion = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GameInfo : INotifyPropertyChanged
    {
        #region Properties

        /// <summary>
        ///    the name of the player that created the game
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        ///    A unique id for a game
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     User picked name of the game
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        ///    should clients listening for game changes autojoin this game?
        /// </summary>
        public bool RequestAutoJoin { get; set; } = false;

        /// <summary>
        ///    has the game been "started"
        /// </summary>
        public bool Started { get; set; }

        /// <summary>
        ///     the Index of the game type
        /// </summary>
        /// <returns></returns>
        /// 
        public int GameIndex { get; set; }

        /// <summary>
        ///    Indicates usage of CitiesAndKnights extensions
        /// </summary>
        /// <returns></returns>
        /// 
        private bool _citiesAndKnights = false;
        public bool CitiesAndKnights
        {
            get
            {
                return _citiesAndKnights;
            }
            set
            {
                if (value != _citiesAndKnights)
                {
                    _citiesAndKnights = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("IsCitiesAndKnights");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return $"[Creator={Creator}][AutoJoin={RequestAutoJoin}][Name={Name}][Id={Id}]";
        }

        #endregion Methods
    }
}