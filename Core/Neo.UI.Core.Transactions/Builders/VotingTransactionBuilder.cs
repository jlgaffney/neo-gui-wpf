using System.Linq;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Builders
{
    internal class VotingTransactionBuilder : ITransactionBuilder<VotingTransactionParameters>
    {
        public Transaction Build(VotingTransactionParameters parameters)
        {
            // TODO Add validation for format of vote strings in parameters
            var votePublicKeys = parameters.Votes.Select(p => ECPoint.Parse(p, ECCurve.Secp256r1)).ToArray();
            
            var transaction = new StateTransaction
            {
                Version = 0,
                Descriptors = new[]
                {
                    new StateDescriptor
                    {
                        Type = StateType.Account,
                        Key = UInt160.Parse(parameters.ScriptHash).ToArray(),
                        Field = "Votes",
                        Value = votePublicKeys.ToByteArray()
                    }
                }
            };

            return transaction;
        }
    }
}
