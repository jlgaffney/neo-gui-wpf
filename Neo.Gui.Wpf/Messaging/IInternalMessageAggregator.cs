using Neo.Gui.Base.Messaging;

namespace Neo.Gui.Wpf.Messaging
{
    public interface IInternalMessageAggregator
    {
        void Subscribe(IMessageHandler messageHandler);

        void Unsubscribe(IMessageHandler messageHandler);

        void Publish<T>(T message);
    }
}