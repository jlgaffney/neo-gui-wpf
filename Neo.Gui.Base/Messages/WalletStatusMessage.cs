using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Status;

namespace Neo.Gui.Base.Messages
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
