using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Neo.Core;
using Neo.IO.Json;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Base.Controls;
using Neo.UI.Base.MVVM;
using Neo.VM;

namespace Neo.UI.Contracts
{
    public class InvokeContractViewModel : ViewModelBase
    {
        private static readonly Fixed8 NetworkFee = Fixed8.FromDecimal(0.001m);

        private InvocationTransaction transaction;

        private UInt160 scriptHash;
        private ContractParameter[] parameters;

        private string scriptHashStr;

        private bool editParametersEnabled;


        private string customScript;
        private string results;

        private string fee;
        
        private bool invokeEnabled;


        

        #region Public Properties

        public string ScriptHash
        {
            get => this.scriptHashStr;
            set
            {
                if (this.scriptHashStr == value) return;

                this.scriptHashStr = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.GetContractEnabled));
            }
        }

        public bool GetContractEnabled => UInt160.TryParse(this.ScriptHash, out _);

        public string ContractName { get; private set; }

        public string ContractVersion { get; private set; }

        public string ContractAuthor { get; private set; }

        public string ContractParameters { get; private set; }

        public bool EditParametersEnabled
        {
            get => this.editParametersEnabled;
            set
            {
                if (this.editParametersEnabled == value) return;

                this.editParametersEnabled = value;

                NotifyPropertyChanged();
            }
        }


        public string CustomScript
        {
            get => this.customScript;
            set
            {
                if (this.customScript == value) return;

                this.customScript = value;

                NotifyPropertyChanged();

                // Update dependent properties
                this.InvokeEnabled = false;
                NotifyPropertyChanged(nameof(this.TestEnabled));
            }
        }

        public string Results
        {
            get => this.results;
            set
            {
                if (this.results == value) return;

                this.results = value;

                NotifyPropertyChanged();
            }
        }

        public string Fee
        {
            get => string.IsNullOrEmpty(this.fee) ? "not evaluated" : this.fee;
            set
            {
                if (this.fee == value) return;

                this.fee = value;

                NotifyPropertyChanged();
            }
        }

        public bool TestEnabled => !string.IsNullOrEmpty(this.CustomScript);

        public bool InvokeEnabled
        {
            get => this.invokeEnabled;
            set
            {
                if (this.invokeEnabled == value) return;

                this.invokeEnabled = value;

                NotifyPropertyChanged();
            }
        }

        #endregion Public Properties

        #region Commands

        public ICommand GetContractCommand => new RelayCommand(this.GetContract);

        public ICommand EditParametersCommand => new RelayCommand(this.EditParameters);

        public ICommand LoadCommand => new RelayCommand(this.Load);

        public ICommand TestCommand => new RelayCommand(this.Test);

        public ICommand InvokeCommand => new RelayCommand(this.Invoke);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        #endregion Commands

        public override void OnWindowAttached(NeoWindow window)
        {
            base.OnWindowAttached(window);

            var invokeContractView = window as InvokeContractView;

            if (invokeContractView == null) return;

            // Check if invocation base transaction was provided
            var tx = invokeContractView.BaseTransaction;

            if (tx == null) return;

            this.transaction = tx;

            invokeContractView.SetSelectedTab(1);
            this.CustomScript = this.transaction.Script.ToHexString();
        }

        public InvocationTransaction GetTransaction()
        {
            if (this.transaction == null) return null;

            var transactionFee = this.transaction.Gas.Equals(Fixed8.Zero) ? NetworkFee : Fixed8.Zero;

            return App.CurrentWallet.MakeTransaction(new InvocationTransaction
            {
                Version = transaction.Version,
                Script = transaction.Script,
                Gas = transaction.Gas,
                Attributes = transaction.Attributes,
                Inputs = transaction.Inputs,
                Outputs = transaction.Outputs
            }, fee: transactionFee);
        }


        private void GetContract()
        {
            this.scriptHash = UInt160.Parse(this.ScriptHash);

            var contract = Blockchain.Default.GetContract(this.scriptHash);

            if (contract == null)
            {
                MessageBox.Show("Cannot find contract.");
                return;
            }

            this.parameters = contract.ParameterList.Select(p => new ContractParameter(p)).ToArray();
            this.ContractName = contract.Name;
            this.ContractVersion = contract.CodeVersion;
            this.ContractAuthor = contract.Author;
            this.ContractParameters = string.Join(", ", contract.ParameterList);

            // Update bindable properties
            NotifyPropertyChanged(nameof(this.ContractName));
            NotifyPropertyChanged(nameof(this.ContractVersion));
            NotifyPropertyChanged(nameof(this.ContractAuthor));
            NotifyPropertyChanged(nameof(this.ContractParameters));

            this.EditParametersEnabled = this.parameters.Length > 0;

            UpdateCustomScript();
        }

        private void EditParameters()
        {
            var view = new ParametersEditorView(this.parameters);
            view.ShowDialog();

            UpdateCustomScript();
        }

        private void UpdateCustomScript()
        {
            if (this.parameters.Any(p => p.Value == null)) return;

            using (var builder = new ScriptBuilder())
            {
                builder.EmitAppCall(this.scriptHash, this.parameters);
                this.CustomScript = builder.ToArray().ToHexString();
            }
        }

        private void Load()
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() != true) return;

            byte[] script = null;
            try
            {
                script = File.ReadAllBytes(openFileDialog.FileName);
            }
            catch {  } // Swallow any exceptions
        
            var scriptHex = string.Empty;

            if (script != null)
            {
                scriptHex = script.ToHexString();
            }

            this.CustomScript = scriptHex;
        }

        private void Test()
        {
            if (this.transaction == null) this.transaction = new InvocationTransaction();

            this.transaction.Version = 1;
            this.transaction.Script = this.CustomScript.HexToBytes();

            // Load default transaction values if required
            if (this.transaction.Attributes == null) this.transaction.Attributes = new TransactionAttribute[0];
            if (this.transaction.Inputs == null) this.transaction.Inputs = new CoinReference[0];
            if (this.transaction.Outputs == null) this.transaction.Outputs = new TransactionOutput[0];
            if (this.transaction.Scripts == null) this.transaction.Scripts = new Witness[0];

            var engine = ApplicationEngine.Run(this.transaction.Script, this.transaction);

            // Get transaction test results
            var builder = new StringBuilder();
            builder.AppendLine($"VM State: {engine.State}");
            builder.AppendLine($"Gas Consumed: {engine.GasConsumed}");
            builder.AppendLine($"Evaluation Stack: {new JArray(engine.EvaluationStack.Select(p => p.ToParameter().ToJson()))}");

            this.Results = builder.ToString();

            if (!engine.State.HasFlag(VMState.FAULT))
            {
                this.transaction.Gas = engine.GasConsumed - Fixed8.FromDecimal(10);

                if (this.transaction.Gas < Fixed8.Zero) this.transaction.Gas = Fixed8.Zero;

                this.transaction.Gas = this.transaction.Gas.Ceiling();

                var transactionFee = this.transaction.Gas.Equals(Fixed8.Zero) ? NetworkFee : this.transaction.Gas;

                this.Fee = transactionFee + " gas";
                this.InvokeEnabled = true;
            }
            else
            {
                MessageBox.Show(Strings.ExecutionFailed);
            }
        }

        private void Invoke()
        {
            // Close window so parent object can get the transaction
            this.TryClose();
        }

        private void Cancel()
        {
            this.transaction = null;

            this.TryClose();
        }
    }
}