using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catan10
{
    public class AckTracker
    {
        #region Delegates + Fields + Events + Enums

        private TaskCompletionSource<object> TCS = new TaskCompletionSource<object>();

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public CatanSignalRClient Client { get; set; }
        public Guid MessageId { get; set; }
        public List<string> PlayerNames { get; set; }
        #endregion Properties

        #region Methods

        public void Cancel()
        {
            Client.OnAck -= Client_OnAck;
        }

        public async Task<bool> WaitForAllAcks(int timeoutMs)
        {
            string s = "";
            PlayerNames.ForEach((p) => s +=p  + ",") ;
            this.TraceMessage($"Waiting for Acks from {s}");
            
            Client.OnAck += Client_OnAck;
            try
            {
                using (new FunctionTimer("Task.TimeoutAfter", true))
                {
                    this.TraceMessage($"Waiting for acks on message: {MessageId}");
                    await TCS.Task.TimeoutAfter(timeoutMs);
                }
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Client.OnAck -= Client_OnAck;
            }
        }

        private void Client_OnAck(string fromPlayer, Guid messageId)
        {
            this.TraceMessage($"Received Ack from {fromPlayer} for message {messageId} IsMyMessage={messageId == this.MessageId}");
            if (messageId == this.MessageId)
            {                
                PlayerNames.Remove(fromPlayer);
                if (PlayerNames.Count == 0)
                {
                    this.TraceMessage($"Received all acks for message {messageId}");
                    TCS.TrySetResult(null);
                }
            }
        }
        #endregion Methods
    }
}