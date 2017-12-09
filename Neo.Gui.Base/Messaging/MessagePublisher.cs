using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Services;

namespace Neo.Gui.Base.Messaging
{
    public class MessagePublisher : IMessagePublisher
    {
        #region Private Fields 
        private readonly IDispatchService dispatchService;
        private readonly IInternalMessageAggregator internalMessageAggregator;
        #endregion

        #region Constructor 
        public MessagePublisher(
            IDispatchService dispatchService,
            IInternalMessageAggregator internalMessageAggregator)
        {
            this.dispatchService = dispatchService;
            this.internalMessageAggregator = internalMessageAggregator;
        }
        #endregion

        #region IMessagePublisher implementation
        public void Publish<T>(T message)
        {
            this.dispatchService.InvokeOnMainUIThread(() =>
            {
                this.internalMessageAggregator.Publish(message);
            });
        }
        #endregion
    }
}
