using System.Linq;

using Neo.SmartContract;

using Neo.UI.Core.Data.TransactionParameters;

namespace Neo.UI.Core.Controllers.TransactionInvokers
{
    internal class DeployContractTransactionInvoker : TransactionInvokerBase
    {
        public override bool IsValid(InvocationTransactionType invocationTransactionType)
        {
            return invocationTransactionType == InvocationTransactionType.DeployContract;
        }

        public override void GenerateTransaction()
        {
            var deployContractTransactionConfiguration = this.Configuration as DeployContractTransactionConfiguration;
            var parameters = deployContractTransactionConfiguration.DeployContractTransactionParameters;

            var script = parameters.ContractSourceCode.HexToBytes();
            var parameterListBytes = string.IsNullOrEmpty(parameters.ParameterList) ? new byte[0] : parameters.ParameterList.HexToBytes();

            ContractParameterType returnType;
            if (string.IsNullOrEmpty(parameters.ReturnType))
            {
                returnType = ContractParameterType.Void;
            }
            else
            {
                returnType = parameters.ReturnType
                    .HexToBytes()
                    .Select(p => (ContractParameterType?)p).FirstOrDefault() ?? ContractParameterType.Void;
            }

            this.Transaction = this.Configuration.WalletController.MakeContractCreationTransaction(
                script,
                parameterListBytes,
                returnType,
                parameters.NeedsStorage,
                parameters.Name,
                parameters.Version,
                parameters.Author,
                parameters.Email,
                parameters.Description);

            this.IsContractTransaction = true;
        }
    }
}
