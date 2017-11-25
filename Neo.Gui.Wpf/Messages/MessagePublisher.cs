using Neo.Gui.Base.Interfaces.Helpers;

namespace Neo.UI.Base.Messages
{
    public class MessagePublisher : IMessagePublisher
    {
        #region Private Fields 
        private readonly IInternalMessageAggregator _messageAggregator;
        private readonly IDispatchHelper dispatchHelper;
        #endregion

        #region Constructor 
        public MessagePublisher(IInternalMessageAggregator messageAggregator, IDispatchHelper dispatchHelper)
        {
            this._messageAggregator = messageAggregator;
            this.dispatchHelper = dispatchHelper;
        }
        #endregion

        #region IMessagePublisher implementation
        public void Publish<T>(T message)
        {
            this.dispatchHelper.InvokeOnMainUIThread(() =>
            {
                this._messageAggregator.Publish(message);
            });
            
        }
        #endregion
    }
}
