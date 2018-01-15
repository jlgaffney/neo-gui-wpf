using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Network;
using Neo.SmartContract;

using Neo.Gui.Globalization.Resources;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Transactions;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.Gui.ViewModels.Transactions
{
    public class SigningViewModel : ViewModelBase, IDialogViewModel<SigningLoadParameters>
    {
        private readonly IClipboardManager clipboardManager;
        private readonly IDialogManager dialogManager;
        private readonly INotificationService notificationService;
        private readonly IWalletController walletController;

        private string input;
        private ContractParametersContext output;
        private bool broadcastVisible;

        public SigningViewModel(
            IClipboardManager clipboardManager,
            IDialogManager dialogManager,
            INotificationService notificationService,
            IWalletController walletController)
        {
            this.clipboardManager = clipboardManager;
            this.dialogManager = dialogManager;
            this.notificationService = notificationService;
            this.walletController = walletController;
        }

        public string Input
        {
            get => this.input;
            set
            {
                if (this.input == value) return;

                this.input = value;

                RaisePropertyChanged();
            }
        }

        public string Output => this.output?.ToString();

        public bool BroadcastVisible
        {
            get => this.broadcastVisible;
            set
            {
                if (this.broadcastVisible == value) return;

                this.broadcastVisible = value;

                RaisePropertyChanged();
            }
        }

        public RelayCommand SignatureCommand => new RelayCommand(this.Sign);

        public RelayCommand BroadcastCommand => new RelayCommand(this.Broadcast);

        public RelayCommand CopyCommand => new RelayCommand(this.Copy);

        public RelayCommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(SigningLoadParameters parameters)
        {
        }
        #endregion

        private void Sign()
        {
            if (string.IsNullOrEmpty(this.Input))
            {
                this.notificationService.ShowErrorNotification(Strings.SigningFailedNoDataMessage);
                return;
            }

            ContractParametersContext context;
            try
            {
                context = ContractParametersContext.Parse(this.Input);
            }
            catch
            {
                this.notificationService.ShowErrorNotification(Strings.SigningFailedNoDataMessage);
                return;
            }

            if (!this.walletController.Sign(context))
            {
                this.notificationService.ShowErrorNotification(Strings.SigningFailedNoDataMessage);
                return;
            }

            this.output = context;
            RaisePropertyChanged(nameof(this.Output));

            if (context.Completed) this.BroadcastVisible = true;
        }

        private void Copy()
        {
            if (this.output == null) return;

            // TODO Highlight output textbox text

            this.clipboardManager.SetText(this.output.ToString());
        }

        private void Broadcast()
        {
            if (this.output == null) return;

            this.output.Verifiable.Scripts = this.output.GetScripts();

            var inventory = (IInventory) this.output.Verifiable;

            this.walletController.Relay(inventory);

            this.dialogManager.ShowInformationDialog(Strings.RelaySuccessTitle, Strings.RelaySuccessText, inventory.Hash.ToString());

            this.BroadcastVisible = false;
        }
    }
}