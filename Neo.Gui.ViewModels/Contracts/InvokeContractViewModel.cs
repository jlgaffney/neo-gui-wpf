using System;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.SmartContract;
using Neo.VM;

using Neo.UI.Core.Globalization.Resources;

using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Dialogs.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Transactions.Parameters;
using Neo.UI.Core.Wallet;

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

        private UInt160 scriptHash;
        private ContractParameter[] contractParameters;

        private string scriptHashStr;

        private string contractName;
        private string contractVersion;
        private string contractAuthor;
        private string contractParametersStr;

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

        public string ContractName
        {
            get => this.contractName;
            private set
            {
                if (this.contractName == value) return;

                this.contractName = value;

                RaisePropertyChanged();
            }
        }

        public string ContractVersion
        {
            get => this.contractVersion;
            private set
            {
                if (this.contractVersion == value) return;

                this.contractVersion = value;

                RaisePropertyChanged();
            }
        }

        public string ContractAuthor
        {
            get => this.contractAuthor;
            private set
            {
                if (this.contractAuthor == value) return;

                this.contractAuthor = value;

                RaisePropertyChanged();
            }
        }

        public string ContractParameters
        {
            get => this.contractParametersStr;
            private set
            {
                if (this.contractParametersStr == value) return;

                this.contractParametersStr = value;

                RaisePropertyChanged();
            }
        }

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

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
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
            this.CustomScript = parameters.Script.ToHexString();
        }
        #endregion

        #region Private Methods
        private async void GetContract()
        {
            var contractState = await this.walletController.GetContractState(this.ScriptHash);

            if (contractState == null)
            {
                this.dialogManager.ShowMessageDialog(string.Empty, "Contract not found!");
                return;
            }

            this.contractParameters = contractState.Parameters.Select(p => new ContractParameter(p)).ToArray();
            this.ContractName = contractState.Name;
            this.ContractVersion = contractState.CodeVersion;
            this.ContractAuthor = contractState.Author;
            this.ContractParameters = string.Join(", ", contractState.Parameters);

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

        private async void Test()
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

            var invokeResult = await this.walletController.InvokeScript(script);

            if (invokeResult == null)
            {
                this.dialogManager.ShowMessageDialog("An error occurred!", Strings.ExecutionFailed);
                return;
            }

            this.Results = invokeResult.Result;
            this.Fee = $"{invokeResult.Fee} GAS";

            this.InvokeEnabled = invokeResult.ExecutionSucceeded;
        }

        private async void Invoke()
        {
            if (!this.InvokeEnabled) return;

            var script = this.CustomScript.Trim().HexToBytes();
            
            var transactionParameters = new InvokeContractTransactionParameters(script);

            await this.walletController.BuildSignAndRelayTransaction(transactionParameters);

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}