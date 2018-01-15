using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;

namespace Neo.UI.Core.Helpers
{
    internal static class TransactionHelper
    {
        public const string ValidatorRegisterApi = "Neo.Validator.Register";
        public const string AssetCreateApi = "Neo.Asset.Create";
        public const string ContractCreateApi = "Neo.Contract.Create";

        public static InvocationTransaction MakeValidatorRegistrationTransaction(ECPoint publicKey)
        {
            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall(ValidatorRegisterApi, publicKey);
                return new InvocationTransaction
                {
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = Contract.CreateSignatureRedeemScript(publicKey).ToScriptHash().ToArray()
                        }
                    },
                    Script = builder.ToArray()
                };
            }
        }

        public static InvocationTransaction MakeAssetCreationTransaction(
            AssetType? assetType,
            string assetName,
            Fixed8 amount,
            byte precision,
            ECPoint assetOwner,
            UInt160 assetAdmin,
            UInt160 assetIssuer)
        {
            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall(AssetCreateApi, assetType, assetName, amount, precision, assetOwner, assetAdmin, assetIssuer);
                return new InvocationTransaction
                {
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = Contract.CreateSignatureRedeemScript(assetOwner).ToScriptHash().ToArray()
                        }
                    },
                    Script = builder.ToArray()
                };
            }
        }

        public static InvocationTransaction MakeContractCreationTransaction(
            byte[] script,
            byte[] parameterList,
            ContractParameterType returnType,
            bool needsStorage,
            string name,
            string version,
            string author,
            string email,
            string description)
        {
            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall(ContractCreateApi, script, parameterList, returnType, needsStorage, name, version, author, email, description);
                return new InvocationTransaction
                {
                    Script = builder.ToArray()
                };
            }
        }

        public static Transaction MakeClaimTransaction(
            CoinReference[] claims,
            UInt256 claimingAssetId,
            Fixed8 claimAmount,
            UInt160 changeAddress)
        {
            return new ClaimTransaction
            {
                Claims = claims,
                Attributes = new TransactionAttribute[0],
                Inputs = new CoinReference[0],
                Outputs = new[]
                {
                    new TransactionOutput
                    {
                        AssetId = claimingAssetId,
                        Value = claimAmount,
                        ScriptHash = changeAddress
                    }
                }
            };
        }

        public static Transaction MakeTransferTransaction(
            IEnumerable<TransferOutput> items,
            IEnumerable<UInt160> accounts,
            string remark,
            UInt160 changeAddress,
            Fixed8 fee)
        {
            var itemList = items.ToList();

            var cOutputs = itemList.Where(p => p.AssetId is UInt160).GroupBy(p => new
            {
                AssetId = (UInt160)p.AssetId,
                Account = p.ScriptHash
            }, (k, g) => new
            {
                k.AssetId,
                Value = g.Aggregate(BigInteger.Zero, (x, y) => x + y.Value.Value),
                k.Account
            }).ToArray();
            Transaction tx;
            var attributes = new List<TransactionAttribute>();
            if (cOutputs.Length == 0)
            {
                tx = new ContractTransaction();
            }
            else
            {
                var accountAddresses = accounts.ToArray();
                var sAttributes = new HashSet<UInt160>();
                using (var builder = new ScriptBuilder())
                {
                    foreach (var output in cOutputs)
                    {
                        byte[] script;
                        using (var builder2 = new ScriptBuilder())
                        {
                            foreach (var address in accountAddresses)
                            {
                                builder2.EmitAppCall(output.AssetId, "balanceOf", address);
                            }

                            builder2.Emit(OpCode.DEPTH, OpCode.PACK);
                            script = builder2.ToArray();
                        }

                        var engine = ApplicationEngine.Run(script);
                        if (engine.State.HasFlag(VMState.FAULT)) return null;

                        var balances = engine.EvaluationStack.Pop().GetArray().Reverse().Zip(accountAddresses, (i, a) => new
                        {
                            Account = a,
                            Value = i.GetBigInteger()
                        }).ToArray();

                        // Check if balance is high enough
                        var sum = balances.Aggregate(BigInteger.Zero, (x, y) => x + y.Value);
                        if (sum < output.Value) return null;

                        if (sum != output.Value)
                        {
                            balances = balances.OrderByDescending(p => p.Value).ToArray();
                            var amount = output.Value;
                            var i = 0;
                            while (balances[i].Value <= amount)
                            {
                                amount -= balances[i++].Value;
                            }

                            balances = amount == BigInteger.Zero
                                ? balances.Take(i).ToArray()
                                : balances.Take(i).Concat(new[] { balances.Last(p => p.Value >= amount) }).ToArray();

                            sum = balances.Aggregate(BigInteger.Zero, (x, y) => x + y.Value);
                        }

                        sAttributes.UnionWith(balances.Select(p => p.Account));

                        for (int i = 0; i < balances.Length; i++)
                        {
                            var value = balances[i].Value;
                            if (i == 0)
                            {
                                var change = sum - output.Value;
                                if (change > 0) value -= change;
                            }
                            builder.EmitAppCall(output.AssetId, "transfer", balances[i].Account, output.Account, value);
                            builder.Emit(OpCode.THROWIFNOT);
                        }
                    }

                    tx = new InvocationTransaction
                    {
                        Version = 1,
                        Script = builder.ToArray()
                    };
                }
                attributes.AddRange(sAttributes.Select(p => new TransactionAttribute
                {
                    Usage = TransactionAttributeUsage.Script,
                    Data = p.ToArray()
                }));
            }

            if (!string.IsNullOrEmpty(remark))
            {
                attributes.Add(new TransactionAttribute
                {
                    Usage = TransactionAttributeUsage.Remark,
                    Data = Encoding.UTF8.GetBytes(remark)
                });
            }

            tx.Attributes = attributes.ToArray();
            tx.Outputs = itemList.Where(p => p.AssetId is UInt256).Select(p => p.ToTxOutput()).ToArray();

            return tx;
        }
    }
}
