using System.Linq;

using Neo.Core;
using Neo.VM;

using Neo.UI.Core.Data.TransactionParameters;
using Neo.UI.Core.Extensions;

namespace Neo.UI.Core.Controllers.TransactionInvokers
{
    internal class VotingTransactionInvoker : TransactionInvokerBase
    {
        public override bool IsValid(InvocationTransactionType invocationTransactionType)
        {
            return invocationTransactionType == InvocationTransactionType.Vote;
        }

        public override void GenerateTransaction()
        {
            var votingTransactionConfiguration = this.Configuration as VotingTransactionConfiguration;
            var parameters = votingTransactionConfiguration.VotingTransactionParameters;

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

                this.Transaction = new InvocationTransaction
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

                this.IsContractTransaction = true;
            }
        }
    }
}
