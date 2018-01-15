namespace Neo.UI.Core.Messaging.Interfaces
{
    internal interface IMessagePublisher
    {
        void Publish<T>(T message);
    }
}
