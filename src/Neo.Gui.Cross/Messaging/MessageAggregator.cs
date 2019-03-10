using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;

namespace Neo.Gui.Cross.Messaging
{
    public class MessageAggregator : IMessageAggregator
    {
        private readonly IList<IMessageHandler> messageHandlers;

        public MessageAggregator()
        {
            messageHandlers = new List<IMessageHandler>();
        }

        public void Subscribe(IMessageHandler messageHandler)
        {
            if (messageHandlers.Contains(messageHandler))
            {
                return;
            }

            messageHandlers.Add(messageHandler);
        }

        public void Unsubscribe(IMessageHandler messageHandler)
        {
            if (!messageHandlers.Contains(messageHandler))
            {
                return;
            }

            messageHandlers.Remove(messageHandler);
        }
        
        public void Publish<T>(T message)
        {
            var messageSubscribers = messageHandlers.OfType<IMessageHandler<T>>().ToList();

            foreach (var item in messageSubscribers)
            {
                Dispatcher.UIThread.InvokeAsync(() => item.HandleMessage(message));
            }
        }
    }
}