using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Neo.Extensions;
using Neo.Network;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Base.MVVM;
using Neo.Wallets;
using Neo.UI.Base.Dialogs;

namespace Neo.UI.Development
{
    public class ContractParametersViewModel : ViewModelBase
    {
        private ContractParametersContext context;
        
        private readonly ObservableCollection<ContractParameter> parameters;

        private string selectedScriptHashAddress;
        private ContractParameter selectedParameter;

        private string currentValue;
        private string newValue;

        private bool showEnabled;
        private bool broadcastVisible;

        public ContractParametersViewModel()
        {
            this.ScriptHashAddresses = new ObservableCollection<string>();
            this.parameters = new ObservableCollection<ContractParameter>();
        }
        
        public ObservableCollection<string> ScriptHashAddresses { get; }

        public ObservableCollection<ContractParameter> Parameters
        {
            get
            {
                this.parameters.Clear();

                if (this.SelectedScriptHashAddress == null) return this.parameters;

                if (this.SelectedScriptHashAddress == string.Empty) return this.parameters;

                if (App.CurrentWallet == null) return this.parameters;

                var scriptHash = Wallet.ToScriptHash(this.SelectedScriptHashAddress);

                if (scriptHash == null) return this.parameters;

                // Get parameters
                this.parameters.AddRange(context.GetParameters(scriptHash));

                return this.parameters;
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

        private void Load()
        {
            var input = InputBox.Show("ParametersContext", "ParametersContext");

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

            this.ScriptHashAddresses.Clear();
            this.Parameters.Clear();
            this.CurrentValue = string.Empty;
            this.NewValue = string.Empty;

            this.ScriptHashAddresses.AddRange(context.ScriptHashes.Select(Wallet.ToAddress));

            this.ShowEnabled = true;

            this.BroadcastVisible = context.Completed;
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