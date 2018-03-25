using System;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Voting;
using Neo.UI.Core.Helpers.Extensions;
using Neo.UI.Core.Transactions.Parameters;
using Neo.UI.Core.Wallet;

namespace Neo.Gui.ViewModels.Voting
{
    public class ElectionViewModel : ViewModelBase, IDialogViewModel<ElectionLoadParameters>
    {
        #region Private Fields 
        private readonly IWalletController walletController;

        private string selectedPublicKey;
        #endregion

        #region Public Properties 
        public ObservableCollection<string> PublicKeys { get; }

        public string SelectedPublicKey
        {
            get => this.selectedPublicKey;
            set
            {
                if (this.selectedPublicKey != null && this.selectedPublicKey.Equals(value)) return;

                this.selectedPublicKey = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.TransactionFee));

                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string TransactionFee
        {
            get
            {
                decimal fee;
                if (string.IsNullOrEmpty(this.SelectedPublicKey))
                {
                    fee = 0;
                }
                else
                {
                    var transactionParameters = new ElectionTransactionParameters(this.SelectedPublicKey);

                    fee = this.walletController.GetTransactionFee(transactionParameters);
                }

                return $"{fee} GAS";
            }
        }

        public bool OkEnabled => this.SelectedPublicKey != null;

        public RelayCommand OkCommand => new RelayCommand(this.HandleOkCommand);
        #endregion

        #region Constructor 
        public ElectionViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;

            this.PublicKeys = new ObservableCollection<string>();
        }
        #endregion
        
        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(ElectionLoadParameters parameters)
        {
            var accountPublicKeys = this.walletController.GetPublicKeysFromStandardAccounts();
            this.PublicKeys.AddRange(accountPublicKeys);
        }
        #endregion

        #region Private Methods 
        private async void HandleOkCommand()
        {
            if (!this.OkEnabled) return;

            var transactionParameters = new ElectionTransactionParameters(this.SelectedPublicKey);

            await this.walletController.BuildSignAndRelayTransaction(transactionParameters);

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}