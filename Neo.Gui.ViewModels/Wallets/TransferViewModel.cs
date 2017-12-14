using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.SmartContract;
using Neo.VM;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Managers;

namespace Neo.Gui.ViewModels.Wallets
{
    public class TransferViewModel : ViewModelBase, IDialogViewModel<TransferDialogResult>
    {
        private readonly IDialogManager dialogManager;
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;

        private string remark = string.Empty;

        public TransferViewModel(
            IDialogManager dialogManager,
            IWalletController walletController,
            IMessagePublisher messagePublisher)
        {
            this.dialogManager = dialogManager;
            this.walletController = walletController;
            this.messagePublisher = messagePublisher;

            this.Items = new ObservableCollection<TransactionOutputItem>();
        }

        public ObservableCollection<TransactionOutputItem> Items { get; }

        public bool OkEnabled => this.Items.Count > 0;

        public ICommand RemarkCommand => new RelayCommand(this.Remark);

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<TransferDialogResult> SetDialogResultAndClose;

        public TransferDialogResult DialogResult { get; private set; }
        #endregion

        private void Remark()
        {
            var result = this.dialogManager.ShowInputDialog(Strings.EnterRemarkTitle, Strings.EnterRemarkMessage, remark);

            if (string.IsNullOrEmpty(result)) return;

            this.remark = result;
        }

        private void Ok()
        {
            var transaction = this.GenerateTransaction();

            if (transaction == null) return;

            var invocationTransaction = transaction as InvocationTransaction;

            if (invocationTransaction != null)
            {
                this.messagePublisher.Publish(new InvokeContractMessage(invocationTransaction));
            }
            else
            {
                this.messagePublisher.Publish(new SignTransactionAndShowInformationMessage(transaction));
            }

            this.Close(this, EventArgs.Empty);
        }

        public void UpdateOkButtonEnabled()
        {
            RaisePropertyChanged(nameof(this.OkEnabled));
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
                var addresses = this.walletController.GetAddresses().ToArray();
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
                tx = this.walletController.MakeTransaction(ctx);
            }

            return tx;
        }
    }
}