using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Input;
using Neo.Core;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Base.Dialogs;
using Neo.UI.Base.MVVM;
using Neo.VM;

namespace Neo.UI.Wallets
{
    internal class TransferViewModel : ViewModelBase
    {
        private string remark = string.Empty;

        private Transaction transaction;

        public TransferViewModel()
        {
            this.Items = new ObservableCollection<TxOutListBoxItem>();
        }

        public ObservableCollection<TxOutListBoxItem> Items { get; }

        public bool OkEnabled => this.Items.Count > 0;

        public ICommand RemarkCommand => new RelayCommand(this.Remark);

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(this.TryClose);


        private void Remark()
        {
            if (InputBox.Show(out var result, Strings.EnterRemarkMessage, Strings.EnterRemarkTitle, remark))
            {
                this.remark = result;
            }
        }

        private void Ok()
        {
            this.transaction = this.GenerateTransaction();

            this.TryClose();
        }

        public void UpdateOkButtonEnabled()
        {
            NotifyPropertyChanged(nameof(this.OkEnabled));
        }

        public Transaction GetTransaction()
        {
            return this.transaction;
        }

        private Transaction GenerateTransaction()
        {
            var cOutputs = this.Items.Where(p => p.AssetId is UInt160).GroupBy(p => new
            {
                AssetId = (UInt160) p.AssetId,
                Account = p.ScriptHash
            }, (k, g) => new
            {
                AssetId = k.AssetId,
                Value = g.Aggregate(BigInteger.Zero, (x, y) => x + y.Value.Value),
                Account = k.Account
            }).ToArray();
            Transaction tx;
            var attributes = new List<TransactionAttribute>();
            if (cOutputs.Length == 0)
            {
                tx = new ContractTransaction();
            }
            else
            {
                var addresses = App.CurrentWallet.GetAddresses().ToArray();
                var sAttributes = new HashSet<UInt160>();
                using (var builder = new ScriptBuilder())
                {
                    foreach (var output in cOutputs)
                    {
                        byte[] script;
                        using (var builder2 = new ScriptBuilder())
                        {
                            foreach (var address in addresses)
                            {
                                builder2.EmitAppCall(output.AssetId, "balanceOf", address);
                            }

                            builder2.Emit(OpCode.DEPTH, OpCode.PACK);
                            script = builder2.ToArray();
                        }

                        var engine = ApplicationEngine.Run(script);
                        if (engine.State.HasFlag(VMState.FAULT)) return null;

                        var balances = engine.EvaluationStack.Pop().GetArray().Reverse().Zip(addresses, (i, a) => new
                        {
                            Account = a,
                            Value = i.GetBigInteger()
                        }).ToArray();

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
                                : balances.Take(i).Concat(new[] {balances.Last(p => p.Value >= amount)}).ToArray();

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
            tx.Outputs = this.Items.Where(p => p.AssetId is UInt256).Select(p => p.ToTxOutput()).ToArray();

            if (tx is ContractTransaction ctx)
            {
                tx = App.CurrentWallet.MakeTransaction(ctx);
            }

            return tx;
        }
    }
}