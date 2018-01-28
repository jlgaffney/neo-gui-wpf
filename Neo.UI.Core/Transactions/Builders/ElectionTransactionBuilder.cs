using Neo.Cryptography.ECC;
using Neo.UI.Core.Data.TransactionParameters;

namespace Neo.UI.Core.Transactions.Builders
{
    internal class ElectionTransactionBuilder : TransactionBuilderBase
    {
        public override bool IsValid(InvocationTransactionType invocationTransactionType)
        {
            return invocationTransactionType == InvocationTransactionType.Election;
        }

        public override void GenerateTransaction()
        {
            var electionTransactionParameters = this.Configuration as ElectionTransactionConfiguration;
            var parameters = electionTransactionParameters.ElectionTransactionParameters;

            var bookKeeperPublicKeyECPoint = ECPoint.Parse(parameters.BookKeeperPublicKey, ECCurve.Secp256k1);
            this.Transaction = this.Configuration.WalletController.MakeValidatorRegistrationTransaction(bookKeeperPublicKeyECPoint);

            this.IsContractTransaction = true;
        }
    }
}
