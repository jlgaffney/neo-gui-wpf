using System;
using System.Linq;
using System.Text;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.IO.Json;
using Neo.SmartContract;
using Neo.VM;

using Neo.Gui.Base.Dialogs.Results.Contracts;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Base.Services;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Globalization;

namespace Neo.Gui.ViewModels.Contracts
{
    public class InvokeContractViewModel :
        ViewModelBase,
        IDialogViewModel<InvokeContractDialogResult>,
        ILoadable
    {
        private static readonly Fixed8 NetworkFee = Fixed8.FromDecimal(0.001m);

        private readonly IDialogManager dialogManager;
        private readonly IFileManager fileManager;
        private readonly IFileDialogService fileDialogService;
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;

        private InvocationTransaction transaction;

        private UInt160 scriptHash;
        private ContractParameter[] parameters;

        private string scriptHashStr;

        private bool editParametersEnabled;


        private string customScript;
        private string results;

        private string fee;
        
        private bool invokeEnabled;

        public InvokeContractViewModel(
            IDialogManager dialogManager,
            IFileManager fileManager,
            IFileDialogService fileDialogService,
            IWalletController walletController,
            IMessagePublisher messagePublisher)
        {
            this.dialogManager = dialogManager;
            this.fileManager = fileManager;
            this.fileDialogService = fileDialogService;
            this.walletController = walletController;
            this.messagePublisher = messagePublisher;
        }

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

        #endregion Public Properties

        #region Commands
        public ICommand GetContractCommand => new RelayCommand(this.GetContract);

        public ICommand EditParametersCommand => new RelayCommand(this.EditParameters);

        public ICommand LoadCommand => new RelayCommand(this.Load);

        public ICommand TestCommand => new RelayCommand(this.Test);

        public ICommand InvokeCommand => new RelayCommand(this.Invoke);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);
        #endregion Commands

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<InvokeContractDialogResult> SetDialogResultAndClose;

        public InvokeContractDialogResult DialogResult { get; private set; }
        #endregion

        #region ILoadable implementation 
        public void OnLoad(params object[] parameters)
        {
            if (!parameters.Any())
            {
                return;
            }

            var invokeContractLoadParameters = parameters[0] as InvokeContractLoadParameters;

            this.SetBaseTransaction(invokeContractLoadParameters.Transaction);
        }
        #endregion

        private  void SetBaseTransaction(InvocationTransaction baseTransaction)
        {
            if (baseTransaction == null) return;

            this.transaction = baseTransaction;

            this.CustomScript = this.transaction.Script.ToHexString();
        }

        public InvocationTransaction GetTransaction()
        {
            if (this.transaction == null) return null;

            var transactionFee = this.transaction.Gas.Equals(Fixed8.Zero) ? NetworkFee : Fixed8.Zero;

            return this.walletController.MakeTransaction(new InvocationTransaction
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

            var contractState = this.walletController.GetContractState(this.scriptHash);

            if (contractState == null)
            {
                this.dialogManager.ShowMessageDialog(string.Empty, "Cannot find contract.");
                return;
            }

            this.parameters = contractState.ParameterList.Select(p => new ContractParameter(p)).ToArray();
            this.ContractName = contractState.Name;
            this.ContractVersion = contractState.CodeVersion;
            this.ContractAuthor = contractState.Author;
            this.ContractParameters = string.Join(", ", contractState.ParameterList);

            // Update bindable properties
            RaisePropertyChanged(nameof(this.ContractName));
            RaisePropertyChanged(nameof(this.ContractVersion));
            RaisePropertyChanged(nameof(this.ContractAuthor));
            RaisePropertyChanged(nameof(this.ContractParameters));

            this.EditParametersEnabled = this.parameters.Length > 0;

            UpdateCustomScript();
        }

        private void EditParameters()
        {
            this.dialogManager.ShowDialog<ContractParametersEditorDialogResult, ContractParametersEditorLoadParameters>(
                new LoadParameters<ContractParametersEditorLoadParameters>(
                    new ContractParametersEditorLoadParameters(this.parameters)));

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
                this.dialogManager.ShowMessageDialog(string.Empty, Strings.ExecutionFailed);
            }
        }

        private void Invoke()
        {
            if (!this.InvokeEnabled) return;

            var tx = this.GetTransaction();

            if (tx == null) return;

            this.messagePublisher.Publish(new SignTransactionAndShowInformationMessage(tx));

            this.Close(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            this.transaction = null;

            this.Close(this, EventArgs.Empty);
        }
    }
}