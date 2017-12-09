using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;
using System.Windows.Input;
using Neo.Cryptography.ECC;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Services;
using Neo.Gui.Wpf.MVVM;
using Neo.SmartContract;

namespace Neo.Gui.Wpf.Views.Contracts
{
    public class ParametersEditorViewModel : ViewModelBase
    {
        private readonly IDispatchService dispatchService;

        /// <summary>
        /// NOTE: Make sure this and the ObservableCollection are kept in sync.
        /// This list has been passed to the view model by reference
        /// </summary>
        private IList<ContractParameter> parameters;

        private DisplayContractParameter selectedParameter;

        private string currentValue;
        private string newValue;

        public ParametersEditorViewModel(IDispatchService dispatchService)
        {
            this.dispatchService = dispatchService;

            this.Parameters = new ObservableCollection<DisplayContractParameter>();
        }
        
        public ObservableCollection<DisplayContractParameter> Parameters { get; }

        public DisplayContractParameter SelectedParameter
        {
            get => this.selectedParameter;
            set
            {
                if (this.selectedParameter == value) return;

                this.selectedParameter = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.RemoveEnabled));
                NotifyPropertyChanged(nameof(this.NewValueEnabled));
                NotifyPropertyChanged(nameof(this.EditArrayEnabled));

                this.CurrentValue = this.SelectedParameter != null
                    ? this.SelectedParameter.Value
                    : string.Empty;

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

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.UpdateEnabled));
                NotifyPropertyChanged(nameof(this.AddEnabled));
            }
        }

        public bool NewValueEnabled => this.SelectedParameter == null || this.SelectedParameter.Parameter.Type != ContractParameterType.Array;
        
        public bool ParameterListEditingEnabled => this.parameters != null && !this.parameters.IsReadOnly;

        public bool AddEnabled => !string.IsNullOrEmpty(this.NewValue) && this.ParameterListEditingEnabled;

        public bool RemoveEnabled => this.SelectedParameter != null && this.ParameterListEditingEnabled;

        public bool EditArrayEnabled => this.SelectedParameter != null && !this.NewValueEnabled;

        public bool UpdateEnabled => this.SelectedParameter != null && !string.IsNullOrEmpty(this.NewValue);

        public ICommand AddCommand => new RelayCommand(this.Add);

        public ICommand RemoveCommand => new RelayCommand(this.Remove);

        public ICommand EditArrayCommand => new RelayCommand(this.EditArray);

        public ICommand UpdateCommand => new RelayCommand(this.Update);


        public async void Load(IList<ContractParameter> parameterList)
        {
            this.parameters = parameterList;

            await this.dispatchService.InvokeOnMainUIThread(() =>
            {
                this.Parameters.Clear();

                if (this.parameters == null) return;

                for (int i = 0; i < this.parameters.Count; i++)
                {
                    this.Parameters.Add(new DisplayContractParameter(i, this.parameters[i]));
                }
            });

            // Update dependent property
            NotifyPropertyChanged(nameof(this.ParameterListEditingEnabled));
            NotifyPropertyChanged(nameof(this.AddEnabled));
            NotifyPropertyChanged(nameof(this.RemoveEnabled));
        }

        private void Add()
        {
            if (string.IsNullOrEmpty(this.NewValue)) return;

            var parameter = ParseParameter(this.NewValue);

            this.dispatchService.InvokeOnMainUIThread(() =>
            {
                var newIndex = this.Parameters.Count;

                var newDisplayParameter = new DisplayContractParameter(newIndex, parameter);

                this.parameters.Add(parameter);
                this.Parameters.Add(newDisplayParameter);

                this.SelectedParameter = newDisplayParameter;
            });
        }

        private void Remove()
        {
            if (this.SelectedParameter == null) return;

            this.dispatchService.InvokeOnMainUIThread(() =>
            {
                this.parameters.RemoveAt(this.SelectedParameter.Index);
                this.Parameters.RemoveAt(this.SelectedParameter.Index);
            });
        }

        private void EditArray()
        {
            if (this.SelectedParameter == null) return;

            var parameter = this.SelectedParameter.Parameter;

            var view = new ParametersEditorView((IList<ContractParameter>) parameter.Value);
            view.ShowDialog();

            // TODO Ensure listview updates with the new Value value
            //listView1.SelectedItems[0].SubItems["value"].Text = parameter.ToString();

            this.CurrentValue = this.SelectedParameter.Value;
        }

        private void Update()
        {
            if (this.SelectedParameter == null || string.IsNullOrEmpty(this.NewValue)) return;

            var parameter = this.SelectedParameter.Parameter;

            parameter.SetValue(this.NewValue);

            // TODO Ensure listview updates with the new Value value
            //listView1.SelectedItems[0].SubItems["value"].Text = parameter.ToString();

            this.CurrentValue = this.SelectedParameter.Value;
            this.NewValue = string.Empty;
        }


        private static ContractParameter ParseParameter(string value)
        {
            var parameter = new ContractParameter();

            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                parameter.Type = ContractParameterType.Boolean;
                parameter.Value = true;
            }
            else if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            {
                parameter.Type = ContractParameterType.Boolean;
                parameter.Value = false;
            }
            else if (long.TryParse(value, out var num))
            {
                parameter.Type = ContractParameterType.Integer;
                parameter.Value = num;
            }
            else if (value.StartsWith("0x"))
            {
                if (UInt160.TryParse(value, out var i160))
                {
                    parameter.Type = ContractParameterType.Hash160;
                    parameter.Value = i160;
                }
                else if (UInt256.TryParse(value, out var i256))
                {
                    parameter.Type = ContractParameterType.Hash256;
                    parameter.Value = i256;
                }
                else if (BigInteger.TryParse(value.Substring(2), NumberStyles.AllowHexSpecifier, null, out BigInteger bi))
                {
                    parameter.Type = ContractParameterType.Integer;
                    parameter.Value = bi;
                }
                else
                {
                    parameter.Type = ContractParameterType.String;
                    parameter.Value = value;
                }
            }
            else if (ECPoint.TryParse(value, ECCurve.Secp256r1, out var point))
            {
                parameter.Type = ContractParameterType.PublicKey;
                parameter.Value = point;
            }
            else
            {
                try
                {
                    parameter.Value = value.HexToBytes();
                    parameter.Type = ContractParameterType.ByteArray;
                }
                catch (FormatException)
                {
                    parameter.Type = ContractParameterType.String;
                    parameter.Value = value;
                }
            }

            return parameter;
        }
    }
}