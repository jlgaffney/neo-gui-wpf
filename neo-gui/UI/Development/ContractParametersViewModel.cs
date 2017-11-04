using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Neo.UI.Base.Extensions;
using Neo.Network;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Base.MVVM;
using Neo.Wallets;
using Neo.UI.Base.Dialogs;
using Neo.UI.Base.Dispatching;

namespace Neo.UI.Development
{
    public class ContractParametersViewModel : ViewModelBase
    {
        private readonly IDispatcher dispatcher;

        private ContractParametersContext context;

        private string selectedScriptHashAddress;
        private ContractParameter selectedParameter;

        private string currentValue;
        private string newValue;

        private bool showEnabled;
        private bool broadcastVisible;

        public ContractParametersViewModel(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;

            this.ScriptHashAddresses = new ObservableCollection<string>();
        }
        
        public ObservableCollection<string> ScriptHashAddresses { get; }

        public ObservableCollection<ContractParameter> Parameters
        {
            get
            {
                var emptyCollection = new ObservableCollection<ContractParameter>();

                if (ApplicationContext.Instance.CurrentWallet == null) return emptyCollection;

                if (string.IsNullOrEmpty(this.SelectedScriptHashAddress)) return emptyCollection;

                var scriptHash = Wallet.ToScriptHash(this.SelectedScriptHashAddress);

                if (scriptHash == null) return emptyCollection;

                // Get parameters
                return new ObservableCollection<ContractParameter>(context.GetParameters(scriptHash));
            }
        }

        public string SelectedScriptHashAddress
        {
            get => this.selectedScriptHashAddress;
            set
            {
                if (this.selectedScriptHashAddress == value) return;

                this.selectedScriptHashAddress = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.Parameters));
            }
        }

        public ContractParameter SelectedParameter
        {
            get => this.selectedParameter;
            set
            {
                if (this.selectedParameter == value) return;

                this.selectedParameter = value;

                NotifyPropertyChanged();

                // Update dependent properties
                if (this.selectedParameter == null) return;

                this.CurrentValue = this.SelectedParameter.ToString();

                this.NewValue = string.Empty;
            }
        }

        public string CurrentValue
        {
            get => this.currentValue;
            set
            {
                if (this.currentValue == value) return;

                this.currentValue = value;

                NotifyPropertyChanged();
            }
        }

        public string NewValue
        {
            get => this.newValue;
            set
            {
                if (this.newValue == value) return;

                this.newValue = value;

                NotifyPropertyChanged();
            }
        }

        public bool ShowEnabled
        {
            get => this.showEnabled;
            set
            {
                if (this.showEnabled == value) return;

                this.showEnabled = value;

                NotifyPropertyChanged();
            }
        }

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

        public ICommand LoadCommand => new RelayCommand(this.Load);

        public ICommand ShowCommand => new RelayCommand(this.Show);

        public ICommand BroadcastCommand => new RelayCommand(this.Broadcast);

        public ICommand UpdateCommand => new RelayCommand(this.Update);

        private async void Load()
        {
            if (!InputBox.Show(out var input, "ParametersContext", "ParametersContext")) return;

            if (string.IsNullOrEmpty(input)) return;

            try
            {
                context = ContractParametersContext.Parse(input);
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            await this.dispatcher.InvokeOnMainUIThread(() =>
            {
                this.ScriptHashAddresses.Clear();
                this.CurrentValue = string.Empty;
                this.NewValue = string.Empty;

                this.ScriptHashAddresses.AddRange(context.ScriptHashes.Select(Wallet.ToAddress));
            });

            this.SelectedScriptHashAddress = null;

            this.ShowEnabled = true;

            this.BroadcastVisible = context.Completed;

            NotifyPropertyChanged(nameof(this.Parameters));
        }

        private void Show()
        {
            InformationBox.Show(context.ToString(), "ParametersContext", "ParametersContext");
        }

        private void Broadcast()
        {
            context.Verifiable.Scripts = context.GetScripts();

            var inventory = (IInventory) context.Verifiable;

            Program.LocalNode.Relay(inventory);

            InformationBox.Show(inventory.Hash.ToString(), Strings.RelaySuccessText, Strings.RelaySuccessTitle);
        }

        private void Update()
        {
            if (this.SelectedScriptHashAddress == null || this.SelectedParameter == null) return;

            this.SelectedParameter.SetValue(this.NewValue);

            this.CurrentValue = this.NewValue;

            this.BroadcastVisible = context.Completed;
        }
    }
}