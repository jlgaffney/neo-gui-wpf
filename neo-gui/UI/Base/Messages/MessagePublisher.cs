using Neo.UI.Base.Dispatching;

namespace Neo.UI.Base.Messages
{
    public class MessagePublisher : IMessagePublisher
    {
        #region Private Fields 
        private readonly IInternalMessageAggregator _messageAggregator;
        private readonly IDispatcher dispatcher;
        #endregion

        #region Constructor 
        public MessagePublisher(IInternalMessageAggregator messageAggregator, IDispatcher dispatcher)
        {
            this._messageAggregator = messageAggregator;
            this.dispatcher = dispatcher;
        }
        #endregion

        #region IMessagePublisher implementation
        public void Publish<T>(T message)
        {
            this.dispatcher.InvokeOnMainUIThread(() =>
            {
                this._messageAggregator.Publish(message);
            });
            
        }
        #endregion
    }
}
