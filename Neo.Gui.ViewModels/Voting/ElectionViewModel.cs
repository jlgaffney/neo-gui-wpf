using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.Cryptography.ECC;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Voting;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.ViewModels.Voting
{
    public class ElectionViewModel : ViewModelBase, IDialogViewModel<ElectionDialogResult>
    {
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;

        private ECPoint selectedBookKeeper;

        public ElectionViewModel(
            IWalletController walletController,
            IMessagePublisher messagePublisher)
        {
            this.walletController = walletController;
            this.messagePublisher = messagePublisher;

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

        public event EventHandler<ElectionDialogResult> SetDialogResultAndClose;

        public ElectionDialogResult DialogResult { get; private set; }
        #endregion

        private void Ok()
        {
            if (!this.OkEnabled) return;

            var transaction = this.MakeTransaction();

            if (transaction == null) return;

            this.messagePublisher.Publish(new InvokeContractMessage(transaction));
            this.Close(this, EventArgs.Empty);
        }

        private InvocationTransaction MakeTransaction()
        {
            return this.walletController.MakeValidatorRegistrationTransaction(this.SelectedBookKeeper);
        }
    }
}