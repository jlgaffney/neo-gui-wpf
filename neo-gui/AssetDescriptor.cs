using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Neo.Core;
using Neo.Properties;
using Neo.SmartContract;
using Neo.VM;

namespace Neo
{
    public class AssetDescriptor
    {
        public UIntBase AssetId;
        public string AssetName;
        public byte Precision;

        public AssetDescriptor(UInt160 assetId)
        {
            byte[] script;
            using (var builder = new ScriptBuilder())
            {
                builder.EmitAppCall(assetId, "decimals");
                builder.EmitAppCall(assetId, "name");
                script = builder.ToArray();
            }
            var engine = ApplicationEngine.Run(script);
            if (engine.State.HasFlag(VMState.FAULT)) throw new ArgumentException();
            this.AssetId = assetId;
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
            if (AssetId is UInt256 assetId)
            {
                return new BigDecimal(App.CurrentWallet.GetAvailable(assetId).GetData(), 8);
            }

            // NEP5
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

        internal static IEnumerable<AssetDescriptor> GetAssets()
        {
            foreach (var assetId in App.CurrentWallet.FindUnspentCoins().Select(p => p.Output.AssetId).Distinct())
            {
                var state = Blockchain.Default.GetAssetState(assetId);
                yield return new AssetDescriptor(state);
            }

            foreach (var s in Settings.Default.NEP5Watched)
            {
                var assetId = UInt160.Parse(s);

                AssetDescriptor asset;
                try
                {
                    asset = new AssetDescriptor(assetId);
                }
                catch (ArgumentException)
                {
                    continue;
                }
                yield return asset;
            }
        }

        public override string ToString()
        {
            return AssetName;
        }
    }
}