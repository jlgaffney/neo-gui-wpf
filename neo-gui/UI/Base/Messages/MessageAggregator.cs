using System.Collections.Generic;

namespace Neo.UI.Base.Messages
{
    public class MessageAggregator : IMessageAggregator
    {
        #region Private Fields 
        private IList<IMessageHandler> _messageHandlers;
        #endregion

        #region Constructor 
        public MessageAggregator()
        {
            this._messageHandlers = new List<IMessageHandler>();
        }
        #endregion

        #region IMessageAggregator implementation 
        public void Subscribe(IMessageHandler messageHandler)
        {
            this._messageHandlers.Add(messageHandler);
        }

        public void Unsubscribe(IMessageHandler messageHandler)
        {
            if (this._messageHandlers.Contains(messageHandler))
            {
                this._messageHandlers.Remove(messageHandler);
            }
        }
        #endregion
    }
}
