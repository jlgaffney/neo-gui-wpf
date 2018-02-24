using System;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Dialogs.LoadParameters.Voting;
using Neo.UI.Core.Helpers.Extensions;
using Neo.UI.Core.Transactions;
using Neo.UI.Core.Transactions.Parameters;
using Neo.UI.Core.Wallet;

namespace Neo.Gui.ViewModels.Voting
{
    public class ElectionViewModel : ViewModelBase, IDialogViewModel<ElectionLoadParameters>
    {
        #region Private Fields 
        private readonly IDialogManager dialogManager;
        private readonly IWalletController walletController;

        private string selectedBookKeeper;
        #endregion

        #region Public Properties 
        public ObservableCollection<string> BookKeepers { get; }

        public string SelectedBookKeeper
        {
            get => this.selectedBookKeeper;
            set
            {
                if (this.selectedBookKeeper != null && this.selectedBookKeeper.Equals(value)) return;

                this.selectedBookKeeper = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public bool OkEnabled => this.SelectedBookKeeper != null;

        public RelayCommand OkCommand => new RelayCommand(this.HandleOkCommand);
        #endregion

        #region Constructor 
        public ElectionViewModel(
            IDialogManager dialogManager,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.walletController = walletController;

            this.BookKeepers = new ObservableCollection<string>();
        }
        #endregion
        
        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(ElectionLoadParameters parameters)
        {
            var bookKeepers = this.walletController.GetPublicKeysFromStandardAccounts();
            this.BookKeepers.AddRange(bookKeepers);
        }
        #endregion

        #region Private Methods 
        private void HandleOkCommand()
        {
            if (!this.OkEnabled) return;

            var transactionParameters = new ElectionTransactionParameters(this.SelectedBookKeeper);

            this.walletController.BuildSignAndRelayTransaction(transactionParameters);

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}