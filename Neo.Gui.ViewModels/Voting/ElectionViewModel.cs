using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Dialogs.LoadParameters.Voting;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.Gui.ViewModels.Voting
{
    public class ElectionViewModel : ViewModelBase, IDialogViewModel<ElectionLoadParameters>
    {
        private readonly IDialogManager dialogManager;
        private readonly IWalletController walletController;

        private ECPoint selectedBookKeeper;

        public ElectionViewModel(
            IDialogManager dialogManager,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.walletController = walletController;

            // Load book keepers
            var bookKeepers = this.walletController.GetStandardAccounts()
                .Select(p => p.GetKey().PublicKey);

            this.BookKeepers = new ObservableCollection<ECPoint>(bookKeepers);
        }

        public ObservableCollection<ECPoint> BookKeepers { get; }

        public ECPoint SelectedBookKeeper
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
        
        public ICommand OkCommand => new RelayCommand(this.Ok);
        
        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(ElectionLoadParameters parameters)
        {
        }
        #endregion

        private void Ok()
        {
            if (!this.OkEnabled) return;

            var transaction = this.MakeTransaction();

            if (transaction == null) return;

            this.dialogManager.ShowDialog(new InvokeContractLoadParameters(transaction));

            this.Close(this, EventArgs.Empty);
        }

        private InvocationTransaction MakeTransaction()
        {
            return this.walletController.MakeValidatorRegistrationTransaction(this.SelectedBookKeeper);
        }
    }
}