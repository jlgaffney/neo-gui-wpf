namespace Neo.Gui.Base.Messaging.Interfaces
{
    internal interface IMessagePublisher
    {
        void Publish<T>(T message);
    }
}
