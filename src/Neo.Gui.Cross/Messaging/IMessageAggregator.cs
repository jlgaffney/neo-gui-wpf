namespace Neo.Gui.Cross.Messaging
{
    public interface IMessageAggregator
    {
        void Subscribe(IMessageHandler messageHandler);

        void Unsubscribe(IMessageHandler messageHandler);

        void Publish<T>(T message);
    }
}
