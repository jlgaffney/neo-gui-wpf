namespace Neo.UI.Base.Messages
{
    public interface IMessageAggregator
    {
        void Subscribe(IMessageHandler messageHandler);

        void Unsubscribe(IMessageHandler messageHandler);

        void Publish<T>(T message);
    }
}