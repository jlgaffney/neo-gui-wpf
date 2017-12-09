using System;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Services;
using Neo.Gui.Wpf.MVVM;
using Neo.Network;
using Neo.SmartContract;
using Neo.UI.Base.Dialogs;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public class SigningViewModel : ViewModelBase, IDialogViewModel<SigningDialogResult>
    {
        private readonly IWalletController walletController;
        private readonly INotificationService notificationService;
        private readonly IClipboardManager clipboardManager;

        private string input;
        private ContractParametersContext output;
        private bool broadcastVisible;

        public SigningViewModel(
            IWalletController walletController,
            IClipboardManager clipboardManager,
            INotificationService notificationService)
        {
            this.walletController = walletController;
            this.notificationService = notificationService;
            this.clipboardManager = clipboardManager;
        }

        public string Input
        {
            get => this.input;
            set
            {
                if (this.input == value) return;

                this.input = value;

                NotifyPropertyChanged();
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

                NotifyPropertyChanged();
            }
        }

        public RelayCommand SignatureCommand => new RelayCommand(this.Sign);

        public RelayCommand BroadcastCommand => new RelayCommand(this.Broadcast);

        public RelayCommand CopyCommand => new RelayCommand(this.Copy);

        public RelayCommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<SigningDialogResult> SetDialogResultAndClose;

        public SigningDialogResult DialogResult { get; private set; }
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
            NotifyPropertyChanged(nameof(this.Output));

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

            InformationBox.Show(inventory.Hash.ToString(), Strings.RelaySuccessText, Strings.RelaySuccessTitle);

            this.BroadcastVisible = false;
        }
    }
}