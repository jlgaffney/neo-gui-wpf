using System;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.VM;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Dialogs.LoadParameters.Voting;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Extensions;

namespace Neo.Gui.ViewModels.Voting
{
    public class VotingViewModel : ViewModelBase,
        IDialogViewModel<VotingLoadParameters>
    {
        private readonly IDialogManager dialogManager;
        private readonly IWalletController walletController;

        private UInt160 scriptHash;

        private string votes;

        public VotingViewModel(
            IDialogManager dialogManager,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.walletController = walletController;
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
        
        public void OnDialogLoad(VotingLoadParameters parameters)
        {
            if (parameters?.ScriptHash == null) return;
            
            this.SetScriptHash(parameters.ScriptHash);
        }
        #endregion
        
        private void SetScriptHash(UInt160 hash)
        {
            this.scriptHash = hash;

            var voteStrings = this.walletController.GetVotes(hash).Select(p => p.ToString()).ToArray();

            // Set address
            this.Address = this.walletController.ScriptHashToAddress(hash);

            // Concatenate votes into multi-line string
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

            this.dialogManager.ShowDialog(new InvokeContractLoadParameters(transaction));

            this.Close(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            this.Close(this, EventArgs.Empty);
        }
    }
}