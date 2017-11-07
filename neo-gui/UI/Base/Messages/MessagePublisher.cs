namespace Neo.UI.Base.Messages
{
    public class MessagePublisher : IMessagePublisher
    {
        #region Private Fields 
        private readonly IInternalMessageAggregator _messageAggregator;
        #endregion

        #region Constructor 
        public MessagePublisher(IInternalMessageAggregator messageAggregator)
        {
            this._messageAggregator = messageAggregator;
        }
        #endregion

        #region IMessagePublisher implementation
        public void Publish<T>(T message)
        {
            this._messageAggregator.Publish(message);
        }
        #endregion
    }
}
