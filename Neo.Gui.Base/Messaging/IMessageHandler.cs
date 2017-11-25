namespace Neo.Gui.Base.Messaging
{
    public interface IMessageHandler {  }

    public interface IMessageHandler<T>  : IMessageHandler
    {
        void HandleMessage(T message);
    }
}