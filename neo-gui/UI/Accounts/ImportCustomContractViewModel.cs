using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.SmartContract;
using Neo.UI.Base.MVVM;
using Neo.Wallets;

namespace Neo.UI.Accounts
{
    public class ImportCustomContractViewModel : ViewModelBase
    {
        private ECPoint selectedRelatedAccount;
        private string parameterList;
        private string script;

        private VerificationContract contract;

        public ImportCustomContractViewModel()
        {
            this.RelatedAccounts = new ObservableCollection<ECPoint>(
                ApplicationContext.Instance.CurrentWallet.GetContracts().Where(p => p.IsStandard).Select(p =>
                    ApplicationContext.Instance.CurrentWallet.GetKey(p.PublicKeyHash).PublicKey));
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
            this.contract = this.GenerateContract();

            this.TryClose();
        }

        public VerificationContract GetContract()
        {
            return this.contract;
        }

        private VerificationContract GenerateContract()
        {
            if (!this.ConfirmEnabled) return null;

            var publicKeyHash = this.SelectedRelatedAccount.EncodePoint(true).ToScriptHash();
            var parameterList = this.ParameterList.HexToBytes().Select(p => (ContractParameterType)p).ToArray();
            var redeemScript = this.Script.HexToBytes();

            return VerificationContract.Create(publicKeyHash, parameterList, redeemScript);
        }
    }
}