using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Wallets;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.ViewModels.Wallets
{
    public class RestoreAccountsViewModel : ViewModelBase, IDialogViewModel<RestoreAccountsDialogResult>
    {
        private readonly IMessagePublisher messagePublisher;

        public RestoreAccountsViewModel(
            IWalletController walletController,
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;

            var keys = walletController.GetKeys().Where(account => walletController.GetContracts(account.PublicKeyHash).All(contract => !contract.IsStandard)).ToList();

            this.Accounts = new ObservableCollection<SelectableVerificationContract>(keys.Select(p => VerificationContract.CreateSignatureContract(p.PublicKey)).Select(p => new SelectableVerificationContract(this, p)));
        }

        public ObservableCollection<SelectableVerificationContract> Accounts { get; }
        
        public bool OkEnabled => this.Accounts.Any(account => account.IsSelected);

        public ICommand OkCommand => new RelayCommand(this.Ok);

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<RestoreAccountsDialogResult> SetDialogResultAndClose;

        public RestoreAccountsDialogResult DialogResult { get; private set; }
        #endregion

        internal void UpdateOkEnabled()
        {
            RaisePropertyChanged(nameof(this.OkEnabled));
        }


        private void Ok()
        {
            var contracts = this.GenerateContracts();

            if (contracts == null) return;

            this.messagePublisher.Publish(new AddContractsMessage(contracts));

            this.Close(this, EventArgs.Empty);
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