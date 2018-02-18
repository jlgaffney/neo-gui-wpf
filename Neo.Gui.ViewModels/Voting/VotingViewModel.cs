using System;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Dialogs.LoadParameters.Voting;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Extensions;
using Neo.UI.Core.Transactions;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.Gui.ViewModels.Voting
{
    public class VotingViewModel : ViewModelBase,
        IDialogViewModel<VotingLoadParameters>
    {
        #region Private Fields 
        private readonly IDialogManager dialogManager;
        private readonly IWalletController walletController;
        
        private string voterAddress;
        private string votes;
        #endregion

        #region Public Properties 
        public string VoterAddress
        {
            get => this.voterAddress;
            set
            {
                if (this.voterAddress == value) return;

                this.voterAddress = value;
                this.RaisePropertyChanged();
            }
        }

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

        public RelayCommand OkCommand => new RelayCommand(this.HandleOkCommand);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
        public VotingViewModel(
            IDialogManager dialogManager,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.walletController = walletController;
        }
        #endregion

        #region ILoadableDialogViewModel Implementation 
        public event EventHandler Close;
        
        public void OnDialogLoad(VotingLoadParameters parameters)
        {
            if (parameters?.VoterScriptHash == null) return;
            
            this.SetVoterScriptHash(parameters.VoterScriptHash);
        }
        #endregion

        #region Private Methods 
        private void SetVoterScriptHash(string scriptHash)
        {
            var voteStrings = this.walletController
                .GetVotes(scriptHash)
                .ToArray();

            // Set voter address
            this.VoterAddress = this.walletController.ScriptHashToAddress(scriptHash);

            // Concatenate votes into multi-line string
            this.Votes = voteStrings.ToMultiLineString();
        }

        //private InvocationTransaction GenerateTransaction()
        //{
        //    using (var builder = new ScriptBuilder())
        //    {
        //        var voteLineCount = 0;

        //        if (!string.IsNullOrEmpty(this.Votes))
        //        {
        //            // Split vote lines
        //            var voteLines = this.Votes.ToLines();

        //            foreach (var line in voteLines.Reverse())
        //            {
        //                builder.EmitPush(line.HexToBytes());
        //            }

        //            voteLineCount = voteLines.Length;
        //        }

        //        builder.EmitPush(voteLineCount);
        //        builder.Emit(OpCode.PACK);
        //        builder.EmitPush(this.scriptHash);
        //        builder.EmitSysCall("Neo.Blockchain.GetAccount");
        //        builder.EmitSysCall("Neo.Account.SetVotes");

        //        return new InvocationTransaction
        //        {
        //            Script = builder.ToArray(),
        //            Attributes = new[]
        //            {
        //                new TransactionAttribute
        //                {
        //                    Usage = TransactionAttributeUsage.Script,
        //                    Data = UInt160.Parse(this.scriptHash).ToArray()
        //                }
        //            }
        //        };
        //    }
        //}

        private void HandleOkCommand()
        {
            //var transaction = this.GenerateTransaction();

            //if (transaction == null) return;

            //this.dialogManager.ShowDialog(new InvokeContractLoadParameters(transaction));

            var voterScriptHash = this.walletController.AddressToScriptHash(this.VoterAddress).ToString();

            var transactionParameters = new VotingTransactionParameters(voterScriptHash, this.Votes.ToLines());

            this.walletController.BuildSignAndRelayTransaction(transactionParameters);

            /*this.dialogManager.ShowDialog(new InvokeContractLoadParameters()
            {
                InvocationTransactionType = InvocationTransactionType.Vote,
                VotingParameters = votingParamers
            });*/

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}