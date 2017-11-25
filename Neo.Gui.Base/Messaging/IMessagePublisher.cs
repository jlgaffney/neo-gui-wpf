namespace Neo.Gui.Base.Messaging
{
    public interface IMessagePublisher
    {
        void Publish<T>(T message);
    }
}
