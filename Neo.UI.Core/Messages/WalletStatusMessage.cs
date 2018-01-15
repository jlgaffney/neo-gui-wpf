using Neo.UI.Core.Status;

namespace Neo.UI.Core.Messages
{
    public class WalletStatusMessage
    {
        public WalletStatusMessage(WalletStatus status)
        {
            this.Status = status;
        }

        public WalletStatus Status { get; }
    }
}
