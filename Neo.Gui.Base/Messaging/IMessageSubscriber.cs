namespace Neo.Gui.Base.Messaging
{
    public interface IMessageSubscriber
    {
        void Subscribe(IMessageHandler messageHandler);

        void Unsubscribe(IMessageHandler messageHandler);
    }
}
