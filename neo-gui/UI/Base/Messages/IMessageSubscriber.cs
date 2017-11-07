namespace Neo.UI.Base.Messages
{
    public interface IMessageSubscriber
    {
        void Subscribe(IMessageHandler messageHandler);

        void Unsubscribe(IMessageHandler messageHandler);
    }
}
