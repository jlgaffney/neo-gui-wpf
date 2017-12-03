using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.Base.Messaging
{
    public class MessagePublisher : IMessagePublisher
    {
        #region Private Fields 
        private readonly IDispatchHelper dispatchHelper;
        private readonly IInternalMessageAggregator internalMessageAggregator;
        #endregion

        #region Constructor 
        public MessagePublisher(
            IDispatchHelper dispatchHelper,
            IInternalMessageAggregator internalMessageAggregator)
        {
            this.dispatchHelper = dispatchHelper;
            this.internalMessageAggregator = internalMessageAggregator;
        }
        #endregion

        #region IMessagePublisher implementation
        public void Publish<T>(T message)
        {
            this.dispatchHelper.InvokeOnMainUIThread(() =>
            {
                this.internalMessageAggregator.Publish(message);
            });
        }
        #endregion
    }
}
