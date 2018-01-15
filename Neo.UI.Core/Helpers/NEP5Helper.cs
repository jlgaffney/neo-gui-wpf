using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Neo.SmartContract;
using Neo.UI.Core.Data;
using Neo.VM;

namespace Neo.UI.Core.Helpers
{
    internal static class NEP5Helper
    {
        public static NEP5AssetItem GetBalance(UInt160 nep5ScriptHash, IList<UInt160> accountScriptHashes)
        {
            byte[] script;
            using (var builder = new ScriptBuilder())
            {
                foreach (var accountScriptHash in accountScriptHashes)
                {
                    builder.EmitAppCall(nep5ScriptHash, "balanceOf", accountScriptHash);
                }
                builder.Emit(OpCode.DEPTH, OpCode.PACK);
                builder.EmitAppCall(nep5ScriptHash, "decimals");
                builder.EmitAppCall(nep5ScriptHash, "name");
                script = builder.ToArray();
            }

            var engine = ApplicationEngine.Run(script);
            if (engine.State.HasFlag(VMState.FAULT)) return null;

            var name = engine.EvaluationStack.Pop().GetString();
            var decimals = (byte)engine.EvaluationStack.Pop().GetBigInteger();
            var amount = engine.EvaluationStack.Pop().GetArray().Aggregate(BigInteger.Zero, (x, y) => x + y.GetBigInteger());

            var balance = new BigDecimal();

            if (amount != 0)
            {
                balance = new BigDecimal(amount, decimals);
            }

            // TODO Set issuer
            return new NEP5AssetItem(nep5ScriptHash, balance)
            {
                Name = name
            };
        }
    }
}
