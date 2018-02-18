using System.Linq;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Builders
{
    public class VotingTransactionBuilder : ITransactionBuilder<VotingTransactionParameters>
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
            

            /*InvocationTransaction transaction;
            using (var builder = new ScriptBuilder())
            {
                var voteLineCount = 0;

                if (!string.IsNullOrEmpty(parameters.Votes))
                {
                    // Split vote lines
                    var voteLines = parameters.Votes.ToLines();

                    foreach (var line in voteLines.Reverse())
                    {
                        builder.EmitPush(line.HexToBytes());
                    }

                    voteLineCount = voteLines.Length;
                }

                builder.EmitPush(voteLineCount);
                builder.Emit(OpCode.PACK);
                builder.EmitPush(parameters.ScriptHash);
                builder.EmitSysCall("Neo.Blockchain.GetAccount");
                builder.EmitSysCall("Neo.Account.SetVotes");

                transaction = new InvocationTransaction
                {
                    Script = builder.ToArray(),
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = UInt160.Parse(parameters.ScriptHash).ToArray()
                        }
                    }
                };
            }*/

            return transaction;
        }
    }
}
