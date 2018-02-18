using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.SmartContract;
using Neo.UI.Core.Extensions;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;
using Neo.VM;
using Neo.Wallets;

namespace Neo.UI.Core.Transactions.Builders
{
    public class AssetRegistrationTransactionBuilder : ITransactionBuilder<AssetRegistrationTransactionParameters>
    {
        private const string AssetCreateApi = "Neo.Asset.Create";

        public Transaction Build(AssetRegistrationTransactionParameters parameters)
        {
            var assetType = parameters.AssetType.ToNeoAssetType();
            var formattedName = parameters.FormattedName;
            var amount = parameters.IsTotalTokenAmountLimited ? Fixed8.Parse(parameters.TotalTokenAmount) : -Fixed8.Satoshi;
            var precisionByte = (byte) parameters.Precision;
            var owner = ECPoint.Parse(parameters.OwnerKey, ECCurve.Secp256r1);
            var admin = Wallet.ToScriptHash(parameters.AdminAddress);
            var issuer = Wallet.ToScriptHash(parameters.IssuerAddress);

            InvocationTransaction transaction;
            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall(AssetCreateApi, assetType, formattedName, amount, precisionByte, owner, admin, issuer);
                transaction = new InvocationTransaction
                {
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = Contract.CreateSignatureRedeemScript(owner).ToScriptHash().ToArray()
                        }
                    },
                    Script = builder.ToArray()
                };
            }

            return transaction;
        }
    }
}
