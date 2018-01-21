using Neo.Core;

namespace Neo.Gui.Dialogs.LoadParameters.Contracts
{
    public class InvokeContractLoadParameters
    {
        public InvocationTransactionType InvocationTransactionType { get; set; }

        public AssetRegistrationParameters AssetRegistrationParameters { get; set; }

        public ElectionParameters ElectionParameters { get; set; }

        public DeployContractParameters DeployContractParameters { get; set; }

        public VotingParameters VotingParameters { get; set; }

        public InvocationTransaction Transaction { get; }

        public InvokeContractLoadParameters(InvocationTransaction transaction)
        {
            this.Transaction = transaction;
        }
    }
}

