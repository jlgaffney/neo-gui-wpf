using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public class ViewContractLoadParameters
    {
        public VerificationContract Contract { get; private set; }

        public ViewContractLoadParameters(VerificationContract contract)
        {
            this.Contract = contract;
        }
    }
}
