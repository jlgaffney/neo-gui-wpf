using System;
using System.Linq;
using System.Numerics;
using Neo.Core;
using Neo.SmartContract;
using Neo.VM;

namespace Neo
{
    internal class AssetDescriptor
    {
        public UIntBase AssetId;
        public string AssetName;
        public byte Precision;

        public AssetDescriptor(UInt160 asset_id)
        {
            byte[] script;
            using (var builder = new ScriptBuilder())
            {
                builder.EmitAppCall(asset_id, "decimals");
                builder.EmitAppCall(asset_id, "name");
                script = builder.ToArray();
            }
            var engine = ApplicationEngine.Run(script);
            if (engine.State.HasFlag(VMState.FAULT)) throw new ArgumentException();
            this.AssetId = asset_id;
            this.AssetName = engine.EvaluationStack.Pop().GetString();
            this.Precision = (byte)engine.EvaluationStack.Pop().GetBigInteger();
        }

        public AssetDescriptor(AssetState state)
        {
            this.AssetId = state.AssetId;
            this.AssetName = state.GetName();
            this.Precision = state.Precision;
        }

        public BigDecimal GetAvailable()
        {
            if (AssetId is UInt256 asset_id)
            {
                return new BigDecimal(App.CurrentWallet.GetAvailable(asset_id).GetData(), 8);
            }

            byte[] script;
            using (var builder = new ScriptBuilder())
            {
                foreach (var account in App.CurrentWallet.GetContracts().Select(p => p.ScriptHash))
                {
                    builder.EmitAppCall((UInt160) AssetId, "balanceOf", account);
                }

                builder.Emit(OpCode.DEPTH, OpCode.PACK);
                script = builder.ToArray();
            }
            var engine = ApplicationEngine.Run(script);
            var amount = engine.EvaluationStack.Pop().GetArray().Aggregate(BigInteger.Zero, (x, y) => x + y.GetBigInteger());
            return new BigDecimal(amount, Precision);
        }

        public override string ToString()
        {
            return AssetName;
        }
    }
}