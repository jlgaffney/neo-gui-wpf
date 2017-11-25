using Neo.Gui.Base.Controllers;

namespace Neo.Gui.Wpf.Messages
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
