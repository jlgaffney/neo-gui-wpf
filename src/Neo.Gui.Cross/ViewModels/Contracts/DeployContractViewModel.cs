using System;
using Neo.Gui.Cross.Services;
using Neo.SmartContract;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Contracts
{
    public class DeployContractViewModel : ViewModelBase
    {
        private readonly IFileDialogService fileDialogService;
        private readonly IFileService fileService;

        private string name;
        private string version;
        private string author;
        private string email;
        private string description;
        private string parameterList;
        private string returnTypeStr;

        private string scriptHex;
        private bool needsStorage;
        private bool needsDynamicCall;


        public DeployContractViewModel(
            IFileDialogService fileDialogService,
            IFileService fileService)
        {
            this.fileDialogService = fileDialogService;
            this.fileService = fileService;
        }
        

        public string Name
        {
            get => name;
            set
            {
                if (Equals(name, value))
                {
                    return;
                }

                name = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(DeployEnabled));
            }
        }

        public string Version
        {
            get => version;
            set
            {
                if (Equals(version, value))
                {
                    return;
                }

                version = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(DeployEnabled));
            }
        }

        public string Author
        {
            get => author;
            set
            {
                if (Equals(author, value))
                {
                    return;
                }

                author = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(DeployEnabled));
            }
        }

        public string Email
        {
            get => email;
            set
            {
                if (Equals(email, value))
                {
                    return;
                }

                email = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(DeployEnabled));
            }
        }

        public string Description
        {
            get => description;
            set
            {
                if (Equals(description, value))
                {
                    return;
                }

                description = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(DeployEnabled));
            }
        }

        public string ParameterList
        {
            get => parameterList;
            set
            {
                if (Equals(parameterList, value))
                {
                    return;
                }

                parameterList = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(DeployEnabled));
            }
        }

        public string ReturnType
        {
            get => returnTypeStr;
            set
            {
                if (Equals(returnTypeStr, value))
                {
                    return;
                }

                returnTypeStr = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(DeployEnabled));
            }
        }

        public string ScriptHex
        {
            get => scriptHex;
            set
            {
                if (Equals(scriptHex, value))
                {
                    return;
                }

                scriptHex = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(ScriptHash));
                this.RaisePropertyChanged(nameof(DeployEnabled));
            }
        }

        public string ScriptHash
        {
            get
            {
                if (string.IsNullOrEmpty(ScriptHex))
                {
                    return string.Empty;
                }

                try
                {
                    var scriptBytes = ScriptHex.HexToBytes();

                    return scriptBytes.ToScriptHash().ToString();
                }
                catch (FormatException)
                {
                    return string.Empty;
                }
            }
        }

        public bool NeedsStorage
        {
            get => needsStorage;
            set
            {
                if (Equals(needsStorage, value))
                {
                    return;
                }

                needsStorage = value;

                this.RaisePropertyChanged();
            }
        }

        public bool NeedsDynamicCall
        {
            get => needsDynamicCall;
            set
            {
                if (Equals(needsDynamicCall, value))
                {
                    return;
                }

                needsDynamicCall = value;

                this.RaisePropertyChanged();
            }
        }

        public bool DeployEnabled =>
            !string.IsNullOrEmpty(Name) &&
            !string.IsNullOrEmpty(Version) &&
            !string.IsNullOrEmpty(Author) &&
            !string.IsNullOrEmpty(Email) &&
            !string.IsNullOrEmpty(Description) &&
            !string.IsNullOrEmpty(ScriptHex);

        public ReactiveCommand LoadCommand => ReactiveCommand.Create(Load);

        public ReactiveCommand DeployCommand => ReactiveCommand.Create(Deploy);

        public ReactiveCommand CancelCommand => ReactiveCommand.Create(OnClose);
        
        


        private async void Load()
        {
            var filePath = await fileDialogService.OpenFileDialog();//"AVM File|*.avm", "avm");

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

            ScriptHex = hexString;
        }

        private async void Deploy()
        {
            // TODO Build transaction, sign it, and relay transaction to the network

            /*var transactionParameters = new DeployContractTransactionParameters(
                this.Name,
                this.Version,
                this.Author,
                this.Email,
                this.Description,
                this.ScriptHex,
                this.ParameterList,
                this.ReturnType,
                this.NeedsStorage,
                this.NeedsDynamicCall);

            await this.walletController.BuildSignAndRelayTransaction(transactionParameters);*/

            OnClose();
        }
    }
}
