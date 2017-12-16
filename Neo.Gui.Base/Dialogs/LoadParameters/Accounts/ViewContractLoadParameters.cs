using Neo.SmartContract;

namespace Neo.Gui.Base.Dialogs.LoadParameters.Accounts
{
    public class ViewContractLoadParameters
    {
        public Contract Contract { get; }

        public ViewContractLoadParameters(Contract contract)
        {
            this.Contract = contract;
        }
    }
}
