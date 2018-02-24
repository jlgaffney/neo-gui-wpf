using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Neo.Core;
using Neo.VM;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Transactions.Exceptions;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Builders
{
    internal class AssetTransferTransactionBuilder : ITransactionBuilder<AssetTransferTransactionParameters>
    {
        private readonly INEP5QueryService nep5QueryService;

        public AssetTransferTransactionBuilder(
            INEP5QueryService nep5QueryService)
        {
            this.nep5QueryService = nep5QueryService;
        }

        public Transaction Build(AssetTransferTransactionParameters parameters)
        {
            var accountScriptHashes = parameters.AccountScriptHashes.Select(UInt160.Parse).ToList();
            var itemList = parameters.TransactionOutputItems;
            var remark = parameters.Remark;
            
            // Get NEP5 asset transfer info
            var nep5Outputs = itemList.Where(p => p.AssetId is UInt160).GroupBy(p => new
            {
                AssetId = (UInt160)p.AssetId,
                Account = p.ScriptHash
            }, (k, g) => new
            {
                k.AssetId,
                Value = g.Aggregate(BigInteger.Zero, (x, y) => x + y.Value.Value),
                k.Account
            }).ToArray();


            Transaction transaction;
            var attributes = new List<TransactionAttribute>();
            if (nep5Outputs.Any())
            {
                // Build NEP5 transfer invocation transaction
                var sAttributes = new HashSet<UInt160>();
                using (var builder = new ScriptBuilder())
                {
                    foreach (var output in nep5Outputs)
                    {
                        var nep5ScriptHash = output.AssetId;

                        // Get balances of each account
                        var balanceDictionary = this.nep5QueryService.GetBalances(nep5ScriptHash, accountScriptHashes);

                        if (balanceDictionary == null)
                        {
                            // TODO Improve this error
                            // TODO Should this actually prevent the transaction from building?
                            throw new Exception("Could not retrieve NEP5 balances!");
                        }

                        var balances = balanceDictionary.Select(keyVal => new
                        {
                            Account = keyVal.Key,
                            Value = keyVal.Value.Value
                        }).ToArray();

                        // Check if total balance is high enough
                        var sum = balances.Aggregate(BigInteger.Zero, (x, y) => x + y.Value);
                        if (sum < output.Value)
                        {
                            throw new InsufficientNEP5BalanceException();
                        }

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

                    transaction = new InvocationTransaction
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
            else
            {
                // No NEP5 assets are being transferred
                transaction = new ContractTransaction();
            }

            if (!string.IsNullOrEmpty(remark))
            {
                attributes.Add(new TransactionAttribute
                {
                    Usage = TransactionAttributeUsage.Remark,
                    Data = Encoding.UTF8.GetBytes(remark)
                });
            }

            transaction.Attributes = attributes.ToArray();

            // Add first-class asset transfer outputs to transaction
            transaction.Outputs = itemList.Where(p => p.AssetId is UInt256).Select(p => p.ToTxOutput()).ToArray();

            return transaction;
        }
    }
}
