using Neo.UI.Core.Data.TransactionParameters;

namespace Neo.Gui.Dialogs.LoadParameters.Contracts
{
    public class InvokeContractLoadParameters
    {
        public InvocationTransactionType InvocationTransactionType { get; set; }

        public AssetRegistrationTransactionParameters AssetRegistrationParameters { get; set; }

        public ElectionTransactionParameters ElectionParameters { get; set; }

        public DeployContractTransactionParameters DeployContractParameters { get; set; }

        public VotingTransactionParameters VotingParameters { get; set; }

        public AssetTransferTransactionParameters AssetTransferParameters { get; set; }

        public InvokeContractLoadParameters()
        {
        }
    }
}

