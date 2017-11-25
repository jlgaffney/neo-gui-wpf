namespace Neo.UI.Base.Messages
{
    public interface IMessagePublisher
    {
        void Publish<T>(T message);
    }
}
