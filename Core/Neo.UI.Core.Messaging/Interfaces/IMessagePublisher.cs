namespace Neo.UI.Core.Messaging.Interfaces
{
    public interface IMessagePublisher
    {
        void Publish<T>(T message);
    }
}
