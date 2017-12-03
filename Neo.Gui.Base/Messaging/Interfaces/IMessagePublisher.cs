namespace Neo.Gui.Base.Messaging.Interfaces
{
    public interface IMessagePublisher
    {
        void Publish<T>(T message);
    }
}
