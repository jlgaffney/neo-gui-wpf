using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Neo.UI.Base.MVVM;
using Neo.Wallets;

namespace Neo.UI.Wallets
{
    public class RestoreAccountsViewModel : ViewModelBase
    {
        private List<VerificationContract> contracts;

        public RestoreAccountsViewModel()
        {
            var keys = App.CurrentWallet.GetKeys();

            keys = keys.Where(account => App.CurrentWallet.GetContracts(account.PublicKeyHash).All(contract => !contract.IsStandard));

            this.Accounts = new ObservableCollection<SelectableVerificationContract>(keys.Select(p => VerificationContract.CreateSignatureContract(p.PublicKey)).Select(p => new SelectableVerificationContract(this, p)));
        }

        public ObservableCollection<SelectableVerificationContract> Accounts { get; }
        
        public bool OkEnabled => this.Accounts.Any(account => account.IsSelected);

        public ICommand OkCommand => new RelayCommand(this.Ok);

        internal void UpdateOkEnabled()
        {
            NotifyPropertyChanged(nameof(this.OkEnabled));
        }


        private void Ok()
        {
            this.contracts = this.GenerateContracts();

            this.TryClose();
        }
        
        public List<VerificationContract> GetContracts()
        {
            return this.contracts;
        }

        private List<VerificationContract> GenerateContracts()
        {
            return this.Accounts.Where(account => account.IsSelected).Select(p => p.Contract).ToList();
        }
    }

    public class SelectableVerificationContract
    {
        private readonly RestoreAccountsViewModel viewModel;
        private readonly VerificationContract contract;

        private bool isSelected;

        internal SelectableVerificationContract(RestoreAccountsViewModel viewModel, VerificationContract contract)
        {
            this.viewModel = viewModel;
            this.contract = contract;
        }

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value) return;

                this.isSelected = value;

                this.viewModel?.UpdateOkEnabled();
            }
        }

        public string Address => this.contract?.Address;

        public VerificationContract Contract => this.contract;
    }
}