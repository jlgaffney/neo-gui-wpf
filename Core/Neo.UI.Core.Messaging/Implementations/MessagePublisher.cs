using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.UI.Core.Messaging.Implementations
{
    internal class MessagePublisher : IMessagePublisher
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
