using System.Collections.Generic;
using System.Linq;

namespace Neo.UI.Base.Messages
{
    public class InternalMessageAggregator : IInternalMessageAggregator
    {
        #region Private Fields 
        private IList<IMessageHandler> _messageHandlers;
        #endregion

        #region Constructor 
        public InternalMessageAggregator()
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

        public void Publish<T>(T message)
        {
            var messageSubscribers = this._messageHandlers.OfType<IMessageHandler<T>>().ToList();

            foreach (var item in messageSubscribers)
            {
                item.HandleMessage(message);
            }
        }
    }
}