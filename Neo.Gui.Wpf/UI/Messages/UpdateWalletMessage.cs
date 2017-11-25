using System;

namespace Neo.UI.Messages
{
    public class UpdateWalletMessage
    {
        public TimeSpan PersistenceSpan { get; private set; }

        public UpdateWalletMessage(TimeSpan persistenceSpan)
        {
            this.PersistenceSpan = persistenceSpan;
        }
    }
}
