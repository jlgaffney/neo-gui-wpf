using System;
using System.Linq;
using System.Text;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.IO.Json;
using Neo.SmartContract;
using Neo.VM;

using Neo.Gui.Globalization.Resources;

using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.Gui.ViewModels.Contracts
{
    public class InvokeContractViewModel : 
        ViewModelBase,
        IDialogViewModel<InvokeContractLoadParameters>
    {
        #region Private Fields 
        private readonly IDialogManager dialogManager;
        private readonly IFileManager fileManager;
        private readonly IFileDialogService fileDialogService;
        private readonly IWalletController walletController;

        private InvocationTransaction transaction;

        private UInt160 scriptHash;
        private ContractParameter[] contractParameters;

        private string scriptHashStr;

        private bool editParametersEnabled;


        private string customScript;
        private string results;

        private string fee;
        
        private bool invokeEnabled;
        #endregion

        #region Public Properties
        public string ScriptHash
        {
            get => this.scriptHashStr;
            set
            {
                if (this.scriptHashStr == value) return;

                this.scriptHashStr = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.GetContractEnabled));
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

                RaisePropertyChanged();
            }
        }

        public string CustomScript
        {
            get => this.customScript;
            set
            {
                if (this.customScript == value) return;

                this.customScript = value;

                RaisePropertyChanged();

                // Update dependent properties
                this.InvokeEnabled = false;
                RaisePropertyChanged(nameof(this.TestEnabled));
            }
        }

        public string Results
        {
            get => this.results;
            set
            {
                if (this.results == value) return;

                this.results = value;

                RaisePropertyChanged();
            }
        }

        public string Fee
        {
            get => string.IsNullOrEmpty(this.fee) ? Strings.NotEvaluated : this.fee;
            set
            {
                if (this.fee == value) return;

                this.fee = value;

                RaisePropertyChanged();
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

                RaisePropertyChanged();
            }
        }

        public RelayCommand GetContractCommand => new RelayCommand(this.GetContract);

        public RelayCommand EditParametersCommand => new RelayCommand(this.EditParameters);

        public RelayCommand LoadCommand => new RelayCommand(this.Load);

        public RelayCommand TestCommand => new RelayCommand(this.Test);

        public RelayCommand InvokeCommand => new RelayCommand(this.Invoke);

        public RelayCommand CancelCommand => new RelayCommand(this.Cancel);
        #endregion Public Properties

        #region Constructor 
        public InvokeContractViewModel(
            IDialogManager dialogManager,
            IFileManager fileManager,
            IFileDialogService fileDialogService,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.fileManager = fileManager;
            this.fileDialogService = fileDialogService;
            this.walletController = walletController;
        }
        #endregion

        #region ILoadableDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(InvokeContractLoadParameters parameters)
        {
            if (parameters?.Transaction == null) return;

            // Set base transaction
            this.transaction = parameters.Transaction;

            this.CustomScript = this.transaction.Script.ToHexString();
        }
        #endregion

        #region Private Methods
        private void GetContract()
        {
            this.scriptHash = UInt160.Parse(this.ScriptHash);

            var contractState = this.walletController.GetContractState(this.scriptHash);

            if (contractState == null)
            {
                this.dialogManager.ShowMessageDialog(string.Empty, "Cannot find contract.");
                return;
            }

            this.contractParameters = contractState.ParameterList.Select(p => new ContractParameter(p)).ToArray();
            this.ContractName = contractState.Name;
            this.ContractVersion = contractState.CodeVersion;
            this.ContractAuthor = contractState.Author;
            this.ContractParameters = string.Join(", ", contractState.ParameterList);

            // Update bindable properties
            RaisePropertyChanged(nameof(this.ContractName));
            RaisePropertyChanged(nameof(this.ContractVersion));
            RaisePropertyChanged(nameof(this.ContractAuthor));
            RaisePropertyChanged(nameof(this.ContractParameters));

            this.EditParametersEnabled = this.contractParameters.Length > 0;

            UpdateCustomScript();
        }

        private void EditParameters()
        {
            this.dialogManager.ShowDialog(new ContractParametersEditorLoadParameters(this.contractParameters));

            UpdateCustomScript();
        }

        private void UpdateCustomScript()
        {
            if (this.contractParameters.Any(p => p.Value == null)) return;

            using (var builder = new ScriptBuilder())
            {
                builder.EmitAppCall(this.scriptHash, this.contractParameters);
                this.CustomScript = builder.ToArray().ToHexString();
            }
        }

        private void Load()
        {
            var filePath = this.fileDialogService.OpenFileDialog();

            if (string.IsNullOrEmpty(filePath)) return;

            byte[] loadedBytes;
            try
            {
                loadedBytes = this.fileManager.ReadAllBytes(filePath);
            }
            catch
            {
                // TODO Show error message
                return;
            }
        
            var hexString = string.Empty;

            if (loadedBytes != null)
            {
                hexString = loadedBytes.ToHexString();
            }

            this.CustomScript = hexString;
        }

        private void Test()
        {
            byte[] script;
            try
            {
                script = this.CustomScript.Trim().HexToBytes();
            }
            catch (FormatException ex)
            {
                this.dialogManager.ShowMessageDialog("An error occurred!", ex.Message);
                return;
            }

            if (this.transaction == null)
            {
                this.transaction = new InvocationTransaction();
            }

            this.transaction.Version = 1;
            this.transaction.Script = script;

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

                var transactionFee = this.transaction.Gas.Equals(Fixed8.Zero) ? this.walletController.NetworkFee : this.transaction.Gas;

                this.Fee = transactionFee + " gas";
                this.InvokeEnabled = true;
            }
            else
            {
                this.dialogManager.ShowMessageDialog(string.Empty, Strings.ExecutionFailed);
            }
        }

        private void Invoke()
        {
            if (!this.InvokeEnabled) return;

            if (this.transaction == null) return;

            this.walletController.InvokeContract(this.transaction);

            this.Close(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            this.transaction = null;

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}