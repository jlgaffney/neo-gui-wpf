using Neo.UI.Core.Messaging.Interfaces;

namespace Neo.UI.Core.Messaging.Implementations
{
    internal class MessageSubscriber : IMessageSubscriber
    {
        #region Private fields 
        private readonly IInternalMessageAggregator internalMessageAggregator;
        #endregion

        #region Constructor
        public MessageSubscriber(IInternalMessageAggregator internalMessageAggregator)
        {
            this.internalMessageAggregator = internalMessageAggregator;
        }
        #endregion

        #region IMessageSubscriber implementation 
        public void Subscribe(IMessageHandler messageHandler)
        {
            this.internalMessageAggregator.Subscribe(messageHandler);
        }

        public void Unsubscribe(IMessageHandler messageHandler)
        {
            this.internalMessageAggregator.Unsubscribe(messageHandler);
        }
        #endregion
    }
}
