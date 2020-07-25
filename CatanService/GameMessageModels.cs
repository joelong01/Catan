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
                From = MainPage.Current.TheHuman.PlayerName,
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

  
}