﻿namespace Neo.Gui.Base.Messaging.Interfaces
{
    public interface IMessageHandler {  }

    public interface IMessageHandler<T>  : IMessageHandler
    {
        void HandleMessage(T message);
    }
}