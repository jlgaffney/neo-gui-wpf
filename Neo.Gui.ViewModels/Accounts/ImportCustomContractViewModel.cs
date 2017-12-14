using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.SmartContract;
using Neo.Wallets;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ImportCustomContractViewModel : ViewModelBase, IDialogViewModel<ImportCustomContractDialogResult>
    {
        private readonly IMessagePublisher messagePublisher;

        private ECPoint selectedRelatedAccount;
        private string parameterList;
        private string script;

        public ImportCustomContractViewModel(
            IWalletController walletController,
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;

            this.RelatedAccounts = new ObservableCollection<ECPoint>(
                walletController.GetContracts().Where(p => p.IsStandard).Select(p =>
                    walletController.GetKey(p.PublicKeyHash).PublicKey));
        }

        public ObservableCollection<ECPoint> RelatedAccounts { get; }

        public ECPoint SelectedRelatedAccount
        {
            get => this.selectedRelatedAccount;
            set
            {
                if (Equals(this.selectedRelatedAccount, value)) return;

                this.selectedRelatedAccount = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public string ParameterList
        {
            get => this.parameterList;
            set
            {
                if (this.parameterList == value) return;

                this.parameterList = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public string Script
        {
            get => this.script;
            set
            {
                if (this.script == value) return;

                this.script = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public bool ConfirmEnabled =>
            this.SelectedRelatedAccount != null &&
            !string.IsNullOrEmpty(this.ParameterList) &&
            !string.IsNullOrEmpty(this.Script);

        public ICommand ConfirmCommand => new RelayCommand(this.Confirm);

        public ICommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<ImportCustomContractDialogResult> SetDialogResultAndClose;

        public ImportCustomContractDialogResult DialogResult { get; private set; }
        #endregion

        private void Confirm()
        {
            var contract = this.GenerateContract();

            if (contract == null) return;

            this.messagePublisher.Publish(new AddContractMessage(contract));

            this.Close(this, EventArgs.Empty);
        }

        private VerificationContract GenerateContract()
        {
            if (!this.ConfirmEnabled) return null;

            var publicKeyHash = this.SelectedRelatedAccount.EncodePoint(true).ToScriptHash();
            var parameters = this.ParameterList.HexToBytes().Select(p => (ContractParameterType)p).ToArray();
            var redeemScript = this.Script.HexToBytes();

            return VerificationContract.Create(publicKeyHash, parameters, redeemScript);
        }
    }
}