using System.Collections.Generic;
using System.Linq;
using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.Base.Messaging
{
    public class InternalMessageAggregator : IInternalMessageAggregator
    {
        #region Private Fields 
        private readonly IList<IMessageHandler> messageHandlers;
        #endregion

        #region Constructor 
        public InternalMessageAggregator()
        {
            this.messageHandlers = new List<IMessageHandler>();
        }
        #endregion

        #region IMessageAggregator implementation 
        public void Subscribe(IMessageHandler messageHandler)
        {
            if (!this.messageHandlers.Contains(messageHandler))
            {
                this.messageHandlers.Add(messageHandler);
            }
        }

        public void Unsubscribe(IMessageHandler messageHandler)
        {
            if (this.messageHandlers.Contains(messageHandler))
            {
                this.messageHandlers.Remove(messageHandler);
            }
        }
        #endregion

        public void Publish<T>(T message)
        {
            var messageSubscribers = this.messageHandlers.OfType<IMessageHandler<T>>().ToList();

            foreach (var item in messageSubscribers)
            {
                item.HandleMessage(message);
            }
        }
    }
}