namespace Neo.UI.Base.Messages
{
    public interface IMessageHandler {  }

    public interface IMessageHandler<T>  : IMessageHandler
    {
        void HandleMessage(T message);
    }
}