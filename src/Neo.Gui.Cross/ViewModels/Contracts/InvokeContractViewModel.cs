using System.Linq;
using Neo.SmartContract;
using Neo.VM;
using ReactiveUI;
using Neo.Gui.Cross.Resources;
using Neo.Gui.Cross.Services;

namespace Neo.Gui.Cross.ViewModels.Contracts
{
    public class InvokeContractViewModel : ViewModelBase
    {
        private readonly IBlockchainService blockchainService;
        private readonly IFileDialogService fileDialogService;
        private readonly IFileService fileService;
        private readonly IWindowService windowService;

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


        public InvokeContractViewModel(
            IBlockchainService blockchainService,
            IFileDialogService fileDialogService,
            IFileService fileService,
            IWindowService windowService)
        {
            this.blockchainService = blockchainService;
            this.fileDialogService = fileDialogService;
            this.fileService = fileService;
            this.windowService = windowService;
        }


        public string ScriptHash
        {
            get => scriptHashStr;
            set
            {
                if (Equals(scriptHashStr, value))
                {
                    return;
                }

                scriptHashStr = value;

                this.RaisePropertyChanged();
                
                this.RaisePropertyChanged(nameof(GetContractEnabled));
            }
        }

        public bool GetContractEnabled => UInt160.TryParse(ScriptHash, out _);

        public string ContractName
        {
            get => contractName;
            private set
            {
                if (Equals(contractName, value))
                {
                    return;
                }

                contractName = value;

                this.RaisePropertyChanged();
            }
        }

        public string ContractVersion
        {
            get => contractVersion;
            private set
            {
                if (Equals(contractVersion, value))
                {
                    return;
                }

                contractVersion = value;

                this.RaisePropertyChanged();
            }
        }

        public string ContractAuthor
        {
            get => contractAuthor;
            private set
            {
                if (Equals(contractAuthor, value))
                {
                    return;
                }

                contractAuthor = value;

                this.RaisePropertyChanged();
            }
        }

        public string ContractParameters
        {
            get => contractParametersStr;
            private set
            {
                if (Equals(contractParametersStr, value))
                {
                    return;
                }

                contractParametersStr = value;

                this.RaisePropertyChanged();
            }
        }

        public bool EditParametersEnabled
        {
            get => editParametersEnabled;
            set
            {
                if (Equals(editParametersEnabled, value))
                {
                    return;
                }

                editParametersEnabled = value;

                this.RaisePropertyChanged();
            }
        }

        public string CustomScript
        {
            get => customScript;
            set
            {
                if (Equals(customScript, value))
                {
                    return;
                }

                customScript = value;

                this.RaisePropertyChanged();

                InvokeEnabled = false;

                this.RaisePropertyChanged(nameof(TestEnabled));
            }
        }

        public string Results
        {
            get => results;
            set
            {
                if (Equals(results, value))
                {
                    return;
                }

                results = value;

                this.RaisePropertyChanged();
            }
        }

        public string Fee
        {
            get => string.IsNullOrEmpty(fee) ? Strings.NotEvaluated : fee;
            set
            {
                if (Equals(fee, value))
                {
                    return;
                }

                fee = value;

                this.RaisePropertyChanged();
            }
        }

        public bool TestEnabled => !string.IsNullOrEmpty(CustomScript);

        public bool InvokeEnabled
        {
            get => invokeEnabled;
            set
            {
                if (Equals(invokeEnabled, value))
                {
                    return;
                }

                invokeEnabled = value;

                this.RaisePropertyChanged();
            }
        }

        public ReactiveCommand GetContractCommand => ReactiveCommand.Create(GetContract);

        public ReactiveCommand EditParametersCommand => ReactiveCommand.Create(EditParameters);

        public ReactiveCommand LoadCommand => ReactiveCommand.Create(Load);

        public ReactiveCommand TestCommand => ReactiveCommand.Create(Test);

        public ReactiveCommand InvokeCommand => ReactiveCommand.Create(Invoke);

        public ReactiveCommand CancelCommand => ReactiveCommand.Create(OnClose);

        


        public void Load(byte[] script)
        {
            if (script == null || script.Length == 0)
            {
                return;
            }

            CustomScript = script.ToHexString();
        }

        private void GetContract()
        {
            if (!GetContractEnabled)
            {
                return;
            }

            scriptHash = UInt160.Parse(ScriptHash);

            var contractState = blockchainService.GetContractState(scriptHash);

            if (contractState == null)
            {
                // TODO Notify user

                return;
            }

            contractParameters = contractState.ParameterList.Select(p => new ContractParameter(p)).ToArray();
            ContractName = contractState.Name;
            ContractVersion = contractState.CodeVersion;
            ContractAuthor = contractState.Author;
            ContractParameters = string.Join(", ", contractState.ParameterList);

            EditParametersEnabled = contractState.ParameterList.Length > 0;

            UpdateCustomScript();
        }

        private void EditParameters()
        {
            // TODO Pass by value instead of reference and receive new array containing edited contract parameters
            windowService.ShowDialog<ContractParametersEditorViewModel, ContractParameter[]>(contractParameters);

            UpdateCustomScript();
        }

        private void UpdateCustomScript()
        {
            if (contractParameters.Any(p => p.Value == null) || !GetContractEnabled)
            {
                return;
            }

            using (var builder = new ScriptBuilder())
            {
                builder.EmitAppCall(scriptHash, contractParameters);

                CustomScript = builder.ToArray().ToHexString();
            }
        }

        private async void Load()
        {
            var filePath = await fileDialogService.OpenFileDialog();

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            byte[] loadedBytes;
            try
            {
                loadedBytes = fileService.ReadAllBytes(filePath);
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

            CustomScript = hexString;
        }

        private async void Test()
        {
            // TODO Implement script test invocation

            /*byte[] script;
            try
            {
                script = CustomScript.Trim().HexToBytes();
            }
            catch (FormatException ex)
            {
                // TODO Inform user
                //this.dialogManager.ShowMessageDialog("An error occurred!", ex.Message);
                return;
            }

            var invokeResult = await this.walletController.InvokeScript(script);

            if (invokeResult == null)
            {
                // TODO Inform user
                //this.dialogManager.ShowMessageDialog("An error occurred!", Strings.ExecutionFailed);
                return;
            }

            Results = invokeResult.Result;
            Fee = $"{invokeResult.Fee} GAS";

            InvokeEnabled = invokeResult.ExecutionSucceeded;*/
        }

        private async void Invoke()
        {
            if (!InvokeEnabled)
            {
                return;
            }

            var script = CustomScript.Trim().HexToBytes();
            
            // TODO Build invocation transaction, sign it, and relay transaction to the network

            /*var transactionParameters = new InvokeContractTransactionParameters(script);

            await this.walletController.BuildSignAndRelayTransaction(transactionParameters);*/

            OnClose();
        }
    }
}
