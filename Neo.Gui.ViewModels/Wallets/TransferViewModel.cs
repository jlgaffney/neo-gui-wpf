using System;
using System.Collections.ObjectModel;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Core;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Dialogs.LoadParameters.Wallets;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data;

namespace Neo.Gui.ViewModels.Wallets
{
    public class TransferViewModel : ViewModelBase, IDialogViewModel<TransferLoadParameters>
    {
        #region Private Fields 
        private readonly IDialogManager dialogManager;
        private readonly IWalletController walletController;

        private bool showAdvancedSection;

        private string fee = "0";

        private string selectedChangeAddress;

        private string remark = string.Empty;
        #endregion

        #region Public Properties 
        public ObservableCollection<TransactionOutputItem> Items { get; }

        public ObservableCollection<string> Addresses { get; }

        public bool ShowAdvancedSection
        {
            get => this.showAdvancedSection;
            set
            {
                if (this.showAdvancedSection == value) return;

                this.showAdvancedSection = value;

                RaisePropertyChanged();
            }
        }

        public string Fee
        {
            get => this.fee;
            set
            {
                if (this.fee == value) return;

                this.fee = value;

                RaisePropertyChanged();
            }
        }

        public string SelectedChangeAddress
        {
            get => this.selectedChangeAddress;
            set
            {
                if (this.selectedChangeAddress == value) return;

                this.selectedChangeAddress = value;

                RaisePropertyChanged();
            }
        }

        public bool OkEnabled => this.Items.Count > 0;

        public RelayCommand RemarkCommand => new RelayCommand(this.Remark);

        public RelayCommand AdvancedCommand => new RelayCommand(this.ToggleAdvancedSection);

        public RelayCommand OkCommand => new RelayCommand(this.Ok);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
        public TransferViewModel(
            IDialogManager dialogManager,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.walletController = walletController;

            this.Items = new ObservableCollection<TransactionOutputItem>();

            this.Addresses = new ObservableCollection<string>(
                this.walletController.GetAccounts().Select(account => account.Address));
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(TransferLoadParameters parameters)
        {
        }
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

        private void ToggleAdvancedSection()
        {
            this.ShowAdvancedSection = !this.ShowAdvancedSection;
        }

        private void Ok()
        {
            if (!this.OkEnabled) return;

            UInt160 transferChangeAddress = null;

            if (!Fixed8.TryParse(this.fee, out var transferFee))
            {
                transferFee = Fixed8.Zero;
            }

            if (!string.IsNullOrEmpty(this.SelectedChangeAddress))
            {
                transferChangeAddress = this.walletController.AddressToScriptHash(this.SelectedChangeAddress);
            }

            var transaction = this.walletController.MakeTransferTransaction(this.Items, this.remark, transferChangeAddress, transferFee);
            
            if (transaction is InvocationTransaction invocationTransaction)
            {
                this.dialogManager.ShowDialog(new InvokeContractLoadParameters(invocationTransaction));
            }
            else
            {
                this.walletController.SignAndRelay(transaction);
            }

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}