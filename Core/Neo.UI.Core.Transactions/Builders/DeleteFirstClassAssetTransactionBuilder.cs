using Neo.Core;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;
using Neo.VM;

namespace Neo.UI.Core.Transactions.Builders
{
    public class DeleteFirstClassAssetTransactionBuilder : ITransactionBuilder<DeleteFirstClassAssetTransactionParameters>
    {
        private readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();

        public Transaction Build(DeleteFirstClassAssetTransactionParameters parameters)
        {
            var assetId = parameters.AssetId;
            var amountToDelete = parameters.Amount;

            return new ContractTransaction
            {
                Outputs = new[]
                {
                    new TransactionOutput
                    {
                        AssetId = assetId,
                        Value = amountToDelete,
                        ScriptHash = this.RecycleScriptHash
                    }
                }
            };
        }
    }
}
