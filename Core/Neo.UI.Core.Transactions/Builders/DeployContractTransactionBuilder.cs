using System.Linq;
using Neo.Core;
using Neo.SmartContract;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;
using Neo.VM;

namespace Neo.UI.Core.Transactions.Builders
{
    internal class DeployContractTransactionBuilder : ITransactionBuilder<DeployContractTransactionParameters>
    {
        private const string ContractCreateApi = "Neo.Contract.Create";

        public Transaction Build(DeployContractTransactionParameters parameters)
        {
            var name = parameters.Name;
            var version = parameters.Version;
            var author = parameters.Author;
            var email = parameters.Email;
            var description = parameters.Description;

            var script = parameters.ScriptHex.HexToBytes();
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

            var properties = ContractPropertyState.NoProperty;
            if (parameters.NeedsStorage)
            {
                properties |= ContractPropertyState.HasStorage;
            }

            if (parameters.NeedsDynamicCall)
            {
                properties |= ContractPropertyState.HasDynamicInvoke;
            }

            InvocationTransaction transaction;
            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall(ContractCreateApi, script, parameterListBytes, returnType, properties, name, version, author, email, description);
                transaction = new InvocationTransaction
                {
                    Script = builder.ToArray()
                };
            }

            return transaction;
        }
    }
}
