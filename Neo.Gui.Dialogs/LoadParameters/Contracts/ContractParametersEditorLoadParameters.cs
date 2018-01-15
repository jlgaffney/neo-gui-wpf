using System.Collections.Generic;
using Neo.SmartContract;

namespace Neo.Gui.Dialogs.LoadParameters.Contracts
{
    public class ContractParametersEditorLoadParameters
    {
        public IList<ContractParameter> ContractParameters { get; }

        public ContractParametersEditorLoadParameters(IList<ContractParameter> contractParameters)
        {
            this.ContractParameters = contractParameters;
        }
    }
}
