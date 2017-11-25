using Neo.Controllers;

namespace Neo.UI.Messages
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
