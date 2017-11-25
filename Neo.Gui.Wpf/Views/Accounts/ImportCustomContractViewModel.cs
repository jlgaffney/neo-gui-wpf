using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Gui.Base.Controllers.Interfaces;
using Neo.Gui.Base.Messaging;
using Neo.Gui.Wpf.Messages;
using Neo.Gui.Wpf.MVVM;
using Neo.SmartContract;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public class ImportCustomContractViewModel : ViewModelBase
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

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public string ParameterList
        {
            get => this.parameterList;
            set
            {
                if (this.parameterList == value) return;

                this.parameterList = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public string Script
        {
            get => this.script;
            set
            {
                if (this.script == value) return;

                this.script = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public bool ConfirmEnabled =>
            this.SelectedRelatedAccount != null &&
            !string.IsNullOrEmpty(this.ParameterList) &&
            !string.IsNullOrEmpty(this.Script);

        public ICommand ConfirmCommand => new RelayCommand(this.Confirm);

        public ICommand CancelCommand => new RelayCommand(this.TryClose);

        private void Confirm()
        {
            var contract = this.GenerateContract();

            if (contract == null) return;

            this.messagePublisher.Publish(new AddContractMessage(contract));
            this.TryClose();
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