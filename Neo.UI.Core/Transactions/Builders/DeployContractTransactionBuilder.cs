using System.Linq;
using Neo.Core;
using Neo.SmartContract;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;
using Neo.VM;

namespace Neo.UI.Core.Transactions.Builders
{
    public class DeployContractTransactionBuilder : ITransactionBuilder<DeployContractTransactionParameters>
    {
        private const string ContractCreateApi = "Neo.Contract.Create";

        public Transaction Build(DeployContractTransactionParameters parameters)
        {
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

            var needsStorage = parameters.NeedsStorage;
            var name = parameters.Name;
            var version = parameters.Version;
            var author = parameters.Author;
            var email = parameters.Email;
            var description = parameters.Description;

            InvocationTransaction transaction;
            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall(ContractCreateApi, script, parameterListBytes, returnType, needsStorage, name, version, author, email, description);
                transaction = new InvocationTransaction
                {
                    Script = builder.ToArray()
                };
            }

            return transaction;
        }
    }
}
