using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.Base.Messaging
{
    public class MessagePublisher : IMessagePublisher
    {
        #region Private Fields 
        private readonly IInternalMessageAggregator internalMessageAggregator;
        #endregion

        #region Constructor 
        public MessagePublisher(IInternalMessageAggregator internalMessageAggregator)
        {
            this.internalMessageAggregator = internalMessageAggregator;
        }
        #endregion

        #region IMessagePublisher implementation
        public void Publish<T>(T message)
        {
            this.internalMessageAggregator.Publish(message);
        }
        #endregion
    }
}
