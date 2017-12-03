using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.Base.Messaging
{
    public class MessageSubscriber : IMessageSubscriber
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
