namespace Neo.UI.Base.Messages
{
    public interface IInternalMessageAggregator
    {
        void Subscribe(IMessageHandler messageHandler);

        void Unsubscribe(IMessageHandler messageHandler);

        void Publish<T>(T message);
    }
}