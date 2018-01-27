using Neo.Cryptography.ECC;

using Neo.UI.Core.Data.TransactionParameters;
using Neo.UI.Core.Extensions;

namespace Neo.UI.Core.Controllers.TransactionInvokers
{
    internal class AssetRegistrationTransactionInvoker : TransactionInvokerBase
    {
        public override bool IsValid(InvocationTransactionType invocationTransactionType)
        {
            return invocationTransactionType == InvocationTransactionType.AssetRegistration;
        }

        public override void GenerateTransaction()
        {
            var assetRegistrationTransactionConfiguration = this.Configuration as AssetRegistrationTransactionConfiguration;
            var parameters = assetRegistrationTransactionConfiguration.AssetRegistrationTransactionParameters;

            var assetType = parameters.AssetType.ToNeoAssetType();
            var formattedName = parameters.FormatedName;
            var amount = parameters.IsTotalTokenAmountLimited ? Fixed8.Parse(parameters.TotalTokenAmount) : -Fixed8.Satoshi;
            var precisionByte = (byte)parameters.Precision;
            var owner = parameters.OwnerKey;
            var admin = this.Configuration.WalletController.AddressToScriptHash(parameters.AdminAddress);
            var issuer = this.Configuration.WalletController.AddressToScriptHash(parameters.IssuerAddress);

            this.Transaction = this.Configuration.WalletController.MakeAssetCreationTransaction(
                assetType, 
                formattedName, 
                amount, 
                precisionByte, 
                ECPoint.Parse(owner, ECCurve.Secp256r1), 
                admin, 
                issuer);

            this.IsContractTransaction = true;
        }
    }
}
