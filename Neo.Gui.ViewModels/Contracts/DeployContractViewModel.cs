using System;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.Gui.Base.Controllers;
using Neo.SmartContract;
using Neo.VM;

using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Contracts;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Services;

namespace Neo.Gui.ViewModels.Contracts
{
    public class DeployContractViewModel : ViewModelBase, IDialogViewModel<DeployContractDialogResult>
    {
        private readonly IFileManager fileManager;
        private readonly IFileDialogService fileDialogService;
        private readonly IMessagePublisher messagePublisher;
        private readonly IWalletController walletController;

        private string name;
        private string version;
        private string author;
        private string email;
        private string description;
        private string parameterList;
        private string returnTypeStr;

        private string code;
        private bool needsStorage;

        public DeployContractViewModel(
            IFileManager fileManager,
            IFileDialogService fileDialogService,
            IMessagePublisher messagePublisher,
            IWalletController walletController)
        {
            this.fileManager = fileManager;
            this.fileDialogService = fileDialogService;
            this.messagePublisher = messagePublisher;
            this.walletController = walletController;
        }


        public string Name
        {
            get => this.name;
            set
            {
                if (this.name == value) return;

                this.name = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string Version
        {
            get => this.version;
            set
            {
                if (this.version == value) return;

                this.version = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string Author
        {
            get => this.author;
            set
            {
                if (this.author == value) return;

                this.author = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string Email
        {
            get => this.email;
            set
            {
                if (this.email == value) return;

                this.email = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string Description
        {
            get => this.description;
            set
            {
                if (this.description == value) return;

                this.description = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string ParameterList
        {
            get => this.parameterList;
            set
            {
                if (this.parameterList == value) return;

                this.parameterList = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string ReturnType
        {
            get => this.returnTypeStr;
            set
            {
                if (this.returnTypeStr == value) return;

                this.returnTypeStr = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string Code
        {
            get => this.code;
            set
            {
                if (this.code == value) return;

                this.code = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.ScriptHash));
                RaisePropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string ScriptHash
        {
            get
            {
                if (string.IsNullOrEmpty(this.Code)) return string.Empty;

                try
                {
                    var codeBytes = this.Code.HexToBytes();

                    var scriptHash = codeBytes.ToScriptHash();

                    return scriptHash.ToString();
                }
                catch (FormatException)
                {
                    return string.Empty;
                }
            }
        }

        public bool NeedsStorage
        {
            get => this.needsStorage;
            set
            {
                if (this.needsStorage == value) return;

                this.needsStorage = value;

                RaisePropertyChanged();
            }
        }

        public bool DeployEnabled =>
            !string.IsNullOrEmpty(this.Name) &&
            !string.IsNullOrEmpty(this.Version) &&
            !string.IsNullOrEmpty(this.Author) &&
            !string.IsNullOrEmpty(this.Email) &&
            !string.IsNullOrEmpty(this.Description) &&
            !string.IsNullOrEmpty(this.Code);

        public ICommand LoadCommand => new RelayCommand(this.Load);

        public ICommand DeployCommand => new RelayCommand(this.Deploy);

        public ICommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<DeployContractDialogResult> SetDialogResultAndClose;

        public DeployContractDialogResult DialogResult { get; private set; }
        #endregion

        private void Load()
        {
            var filePath = this.fileDialogService.OpenFileDialog("AVM File|*.avm", "avm");

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

            this.Code = hexString;
        }

        private void Deploy()
        {
            var transaction = this.MakeTransaction();

            if (transaction == null) return;

            this.messagePublisher.Publish(new InvokeContractMessage(transaction));
            this.Close(this, EventArgs.Empty);
        }

        private InvocationTransaction MakeTransaction()
        {
            if (!this.DeployEnabled) return null;

            var script = this.Code.HexToBytes();
            var parameterListBytes = string.IsNullOrEmpty(this.ParameterList) ? new byte[0] : this.ParameterList.HexToBytes();

            ContractParameterType returnType;
            if (string.IsNullOrEmpty(this.ReturnType))
            {
                returnType = ContractParameterType.Void;
            }
            else
            {
                returnType = this.ReturnType.HexToBytes().Select(p => (ContractParameterType?)p).FirstOrDefault() ?? ContractParameterType.Void;
            }

            return this.walletController.MakeContractCreationTransaction(script, parameterListBytes, returnType,
                this.NeedsStorage, this.Name, this.Version, this.Author, this.Email, this.Description);
        }
    }
}