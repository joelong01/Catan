using System;

using Catan.Proxy;

namespace Catan10.CatanService
{
    public class CreateGameModel : LogHeader
    {

        public GameInfo GameInfo { get; set; }

        public static CatanMessage CreateMessage(GameInfo gameInfo)
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


    }


    public class AckModel: LogHeader
    {
        public string Player { get; set; }
        public Guid MessageId { get; set; }

        public static CatanMessage CreateMessage(string player, Guid ackdMessageId)
        {

            var model = new AckModel
            {
                Action = CatanAction.Ack,
                CanUndo = false,
                CatanGame = CatanGames.Regular,
                LogId = default,
                LogType = LogType.Normal,
                NewState = MainPage.Current.CurrentGameState,
                OldState = MainPage.Current.CurrentGameState,
                Previous = null,
                SentBy = MainPage.Current.TheHuman,
                CreatedTime = DateTime.Now,
                TypeName = typeof(AckModel).FullName,
                Player = player,
                MessageId = ackdMessageId

            };



            var message = new CatanMessage
            {
                ActionType = ActionType.Normal,
                Data = (object)model,
                DataTypeName = typeof(AckModel).FullName,
                From = player,
                MessageId = Guid.Empty,
                MessageType = MessageType.Ack,
                Sequence = 0,
                To = ""
            };
            return message;
        }
    }
}
