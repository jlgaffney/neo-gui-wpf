using System;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.VM;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Voting;
using Neo.Gui.Base.Dialogs.Results.Voting;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.ViewModels.Voting
{
    public class VotingViewModel : ViewModelBase,
        ILoadableDialogViewModel<VotingDialogResult, VotingLoadParameters>
    {
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;

        private UInt160 scriptHash;

        private string votes;

        public VotingViewModel(
            IWalletController walletController,
            IMessagePublisher messagePublisher)
        {
            this.walletController = walletController;
            this.messagePublisher = messagePublisher;
        }

        public string Address { get; private set; }

        public string Votes
        {
            get => this.votes;
            set
            {
                if (this.votes == value) return;

                this.votes = value;

                RaisePropertyChanged();
            }
        }

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        #region ILoadableDialogViewModel Implementation 
        public event EventHandler Close;

        public event EventHandler<VotingDialogResult> SetDialogResultAndClose;

        public VotingDialogResult DialogResult { get; set; }
        
        public void OnDialogLoad(VotingLoadParameters parameters)
        {
            if (parameters?.ScriptHash == null) return;
            
            this.SetScriptHash(parameters.ScriptHash);
        }
        #endregion

        private void SetScriptHash(UInt160 hash)
        {
            this.scriptHash = hash;

            var account = this.walletController.GetAccountState(hash);

            // Set address
            this.Address = this.walletController.ToAddress(hash);

            // Concatenate votes into multi-line string
            var voteStrings = account.Votes.Select(p => p.ToString()).ToArray();

            // Set votes
            this.Votes = voteStrings.ToMultiLineString();

            // Update bindable properties
            RaisePropertyChanged(nameof(this.Address));
        }

        private InvocationTransaction GenerateTransaction()
        {
            using (var builder = new ScriptBuilder())
            {
                var voteLineCount = 0;

                if (!string.IsNullOrEmpty(this.Votes))
                {
                    // Split vote lines
                    var voteLines = this.Votes.ToLines();

                    foreach (var line in voteLines.Reverse())
                    {
                        builder.EmitPush(line.HexToBytes());
                    }

                    voteLineCount = voteLines.Length;
                }

                builder.EmitPush(voteLineCount);
                builder.Emit(OpCode.PACK);
                builder.EmitPush(this.scriptHash);
                builder.EmitSysCall("Neo.Blockchain.GetAccount");
                builder.EmitSysCall("Neo.Account.SetVotes");

                return new InvocationTransaction
                {
                    Script = builder.ToArray(),
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = this.scriptHash.ToArray()
                        }
                    }
                };
            }
        }

        private void Ok()
        {
            var transaction = this.GenerateTransaction();

            if (transaction == null) return;

            this.messagePublisher.Publish(new InvokeContractMessage(transaction));

            this.Close(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            this.Close(this, EventArgs.Empty);
        }
    }
}