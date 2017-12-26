using System;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;

namespace Neo.Gui.ViewModels.Wallets
{
    public class ClaimViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<WalletStatusMessage>,
        IDialogViewModel<ClaimDialogResult>
    {
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;
        private readonly IMessageSubscriber messageSubscriber;

        private Fixed8 availableGas = Fixed8.Zero;
        private Fixed8 unavailableGas = Fixed8.Zero;

        private bool claimEnabled;



        public ClaimViewModel(
            IWalletController walletController,
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber)
        {
            this.walletController = walletController;
            this.messagePublisher = messagePublisher;
            this.messageSubscriber = messageSubscriber;
        }

        #region Public Properties

        public Fixed8 AvailableGas
        {
            get => this.availableGas;
            set
            {
                if (this.availableGas == value) return;

                this.availableGas = value;

                RaisePropertyChanged();
            }
        }

        public Fixed8 UnavailableGas
        {
            get => this.unavailableGas;
            set
            {
                if (this.unavailableGas == value) return;

                this.unavailableGas = value;

                RaisePropertyChanged();
            }
        }

        public bool ClaimEnabled
        {
            get => this.claimEnabled;
            set
            {
                if (this.claimEnabled == value) return;

                this.claimEnabled = value;

                RaisePropertyChanged();
            }
        }

        #endregion Public Properties

        public ICommand ClaimCommand => new RelayCommand(this.Claim);

        #region IDialogViewModel Implementation 
        public event EventHandler Close;

        public event EventHandler<ClaimDialogResult> SetDialogResultAndClose;

        public ClaimDialogResult DialogResult { get; set; }
        #endregion

        #region ILoadable implementation
        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);

            this.CalculateBonusAvailable();
        }
        #endregion

        #region IUnloadable implementation
        public void OnUnload()
        {
            this.messageSubscriber.Unsubscribe(this);
        }
        #endregion

        private void CalculateBonusAvailable()
        {
            var bonusAvailable = this.walletController.CalculateBonus();
            this.AvailableGas = bonusAvailable;

            if (bonusAvailable == Fixed8.Zero)
            {
                this.ClaimEnabled = false;
            }
        }

        private void CalculateBonusUnavailable(uint height)
        {
           this.UnavailableGas = this.walletController.CalculateUnavailableBonusGas(height);
        }        

        private void Claim()
        {
            var claims = this.walletController.GetUnclaimedCoins().Select(p => p.Reference).ToArray();

            if (claims.Length == 0) return;

            var transaction = this.walletController.MakeClaimTransaction(claims);

            this.messagePublisher.Publish(new SignTransactionAndShowInformationMessage(transaction));

            this.Close(this, EventArgs.Empty);
        }

        #region IMessageHandler implementation

        public void HandleMessage(WalletStatusMessage message)
        {
            this.CalculateBonusUnavailable(message.Status.BlockchainStatus.Height + 1);
        }

        #endregion
    }
}