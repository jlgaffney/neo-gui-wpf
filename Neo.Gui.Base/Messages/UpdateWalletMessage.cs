using System;

namespace Neo.Gui.Base.Messages
{
    public class UpdateWalletMessage
    {
        public UpdateWalletMessage(TimeSpan persistenceSpan)
        {
            this.PersistenceSpan = persistenceSpan;
        }

        public TimeSpan PersistenceSpan { get; }
    }
}
