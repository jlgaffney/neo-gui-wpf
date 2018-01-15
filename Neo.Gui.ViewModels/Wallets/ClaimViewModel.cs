using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Wallets;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Messages;
using Neo.UI.Core.Messaging.Interfaces;

namespace Neo.Gui.ViewModels.Wallets
{
    public class ClaimViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<WalletStatusMessage>,
        IDialogViewModel<ClaimLoadParameters>
    {
        #region Private Fields 
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IWalletController walletController;

        private Fixed8 availableGas = Fixed8.Zero;
        private Fixed8 unavailableGas = Fixed8.Zero;

        private bool claimEnabled;
        #endregion

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

        public RelayCommand ClaimCommand => new RelayCommand(this.Claim);
        #endregion Public Properties

        #region Constructor 
        public ClaimViewModel(
            IMessageSubscriber messageSubscriber,
            IWalletController walletController)
        {
            this.messageSubscriber = messageSubscriber;
            this.walletController = walletController;
        }
        #endregion

        #region IDialogViewModel Implementation 
        public event EventHandler Close;

        public void OnDialogLoad(ClaimLoadParameters parameters)
        {
        }
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

        #region IMessageHandler implementation
        public void HandleMessage(WalletStatusMessage message)
        {
            this.CalculateBonusUnavailable(message.Status.BlockchainStatus.Height + 1);
        }

        #endregion

        #region Private Methods 
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
            this.walletController.ClaimUtilityTokenAsset();

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}