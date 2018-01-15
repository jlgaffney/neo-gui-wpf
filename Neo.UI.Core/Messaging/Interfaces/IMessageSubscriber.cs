namespace Neo.UI.Core.Messaging.Interfaces
{
    public interface IMessageSubscriber
    {
        void Subscribe(IMessageHandler messageHandler);

        void Unsubscribe(IMessageHandler messageHandler);
    }
}
