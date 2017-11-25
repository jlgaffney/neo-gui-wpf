using Neo.Gui.Base.Messaging;

namespace Neo.Gui.Wpf.Messaging
{
    public class MessageSubscriber : IMessageSubscriber
    {
        #region Private fields 
        private readonly IInternalMessageAggregator _internalMessageAggregator;
        #endregion

        #region Constructor
        public MessageSubscriber(IInternalMessageAggregator internalMessageAggregator)
        {
            this._internalMessageAggregator = internalMessageAggregator;
        }
        #endregion

        #region IMessageSubscriber implementation 
        public void Subscribe(IMessageHandler messageHandler)
        {
            this._internalMessageAggregator.Subscribe(messageHandler);
        }

        public void Unsubscribe(IMessageHandler messageHandler)
        {
            this._internalMessageAggregator.Unsubscribe(messageHandler);
        }
        #endregion
    }
}
