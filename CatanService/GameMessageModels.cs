using System;

using Catan.Proxy;

namespace Catan10.CatanService
{
    public class AckModel
    {
        #region Properties

        public Guid AckedMessageId { get; set; }

        #endregion Properties

        #region Methods

        public static CatanMessage CreateMessage (CatanMessage msgToAck)
        {
            var model = new AckModel()
            {
                AckedMessageId = msgToAck.MessageId
            };

            var message = new CatanMessage
            {
                ActionType = ActionType.Normal,
                Data = (object)model,
                DataTypeName = typeof(AckModel).FullName,
                From = msgToAck.From,
                MessageType = MessageType.Ack,
                Sequence = 0,
                GameInfo = msgToAck.GameInfo,
                To = "*"
            };
            return message;
        }

        public override string ToString ()
        {
            return $"Ack={AckedMessageId}";
        }

        #endregion Methods
    }

    public class CreateGameModel : LogHeader
    {
        #region Properties

        public GameInfo GameInfo { get; set; }

        #endregion Properties

        #region Methods

        public static CatanMessage CreateMessage (GameInfo gameInfo)
        {
            var model = new CreateGameModel
            {
                Action = CatanAction.GameCreated,
                CanUndo = false,
                CatanGame = CatanGames.Regular,
                LogId = default,
                LogType = LogType.Normal,
                NewState = GameState.WaitingForPlayers,
                OldState = GameState.Uninitialized,
                Previous = null,
                SentBy = MainPage.Current.TheHuman,
                CreatedTime = DateTime.Now,
                TypeName = typeof(CreateGameModel).FullName,
                GameInfo = gameInfo
            };

            var message = new CatanMessage
            {
                ActionType = ActionType.Normal,
                Data = (object)model,
                DataTypeName = typeof(CreateGameModel).FullName,
                From = "",
                MessageId = default,
                MessageType = MessageType.BroadcastMessage,
                Sequence = 0,
                To = ""
            };
            return message;
        }

        #endregion Methods
    }
}