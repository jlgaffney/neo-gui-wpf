using Neo.Wallets;

namespace Neo.Gui.Base.Dialogs.LoadParameters.Accounts
{
    public class ViewContractLoadParameters
    {
        public VerificationContract Contract { get; }

        public ViewContractLoadParameters(VerificationContract contract)
        {
            this.Contract = contract;
        }
    }
}
