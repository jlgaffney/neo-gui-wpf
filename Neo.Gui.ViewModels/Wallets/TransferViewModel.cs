using System;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.Base.Managers;

namespace Neo.Gui.ViewModels.Wallets
{
    public class TransferViewModel : ViewModelBase, IDialogViewModel<TransferDialogResult>
    {
        #region Private Fields 
        private readonly IDialogManager dialogManager;
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;

        private string remark = string.Empty;
        #endregion

        #region Public Properties 
        public ObservableCollection<TransactionOutputItem> Items { get; }

        public bool OkEnabled => this.Items.Count > 0;

        public RelayCommand RemarkCommand => new RelayCommand(this.Remark);

        public RelayCommand OkCommand => new RelayCommand(this.Ok);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
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
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<TransferDialogResult> SetDialogResultAndClose;

        public TransferDialogResult DialogResult { get; private set; }
        #endregion

        #region Public Methods 
        public void UpdateOkButtonEnabled()
        {
            // TODO: Issue #109 [AboimPinto]: Having a public method in ViewModel is a "smell" that this has not been used as should be.

            RaisePropertyChanged(nameof(this.OkEnabled));
        }
        #endregion

        #region Private Methods 
        private void Remark()
        {
            var result = this.dialogManager.ShowInputDialog(Strings.EnterRemarkTitle, Strings.EnterRemarkMessage, remark);

            if (string.IsNullOrEmpty(result)) return;

            this.remark = result;
        }

        private void Ok()
        {
            this.walletController.ExecuteTransferTransaction(this.Items, this.remark);

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}