using System;
using System.Windows;
using System.Windows.Input;
using Neo.Network;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Base.Dialogs;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Transactions
{
    public class SignatureViewModel : ViewModelBase
    {
        private string input;
        private ContractParametersContext output;

        private bool broadcastVisible;

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

        public ICommand SignatureCommand => new RelayCommand(this.Sign);

        public ICommand BroadcastCommand => new RelayCommand(this.Broadcast);

        public ICommand CopyCommand => new RelayCommand(this.Copy);

        public ICommand CloseCommand => new RelayCommand(this.TryClose);

        private void Sign()
        {
            if (string.IsNullOrEmpty(this.Input))
            {
                MessageBox.Show(Strings.SigningFailedNoDataMessage);
                return;
            }

            ContractParametersContext context;
            try
            {
                context = ContractParametersContext.Parse(this.Input);
            }
            catch
            {
                MessageBox.Show(Strings.SigningFailedNoDataMessage);
                return;
            }

            if (!App.CurrentWallet.Sign(context))
            {
                MessageBox.Show(Strings.SigningFailedKeyNotFoundMessage);
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

            Clipboard.SetText(this.output.ToString());
        }

        private void Broadcast()
        {
            if (this.output == null) return;

            this.output.Verifiable.Scripts = this.output.GetScripts();

            var inventory = (IInventory) this.output.Verifiable;

            Program.LocalNode.Relay(inventory);

            InformationBox.Show(inventory.Hash.ToString(), Strings.RelaySuccessText, Strings.RelaySuccessTitle);

            this.BroadcastVisible = false;
        }
    }
}