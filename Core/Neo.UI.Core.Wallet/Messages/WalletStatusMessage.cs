using Neo.UI.Core.Data;

namespace Neo.UI.Core.Wallet.Messages
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
