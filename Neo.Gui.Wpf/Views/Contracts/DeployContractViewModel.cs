using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Neo.Core;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Wpf.MVVM;
using Neo.SmartContract;
using Neo.VM;

namespace Neo.Gui.Wpf.Views.Contracts
{
    public class DeployContractViewModel : ViewModelBase
    {
        private readonly IMessagePublisher messagePublisher;

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
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;
        }


        public string Name
        {
            get => this.name;
            set
            {
                if (this.name == value) return;

                this.name = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string Version
        {
            get => this.version;
            set
            {
                if (this.version == value) return;

                this.version = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string Author
        {
            get => this.author;
            set
            {
                if (this.author == value) return;

                this.author = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string Email
        {
            get => this.email;
            set
            {
                if (this.email == value) return;

                this.email = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string Description
        {
            get => this.description;
            set
            {
                if (this.description == value) return;

                this.description = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string ParameterList
        {
            get => this.parameterList;
            set
            {
                if (this.parameterList == value) return;

                this.parameterList = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string ReturnType
        {
            get => this.returnTypeStr;
            set
            {
                if (this.returnTypeStr == value) return;

                this.returnTypeStr = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.DeployEnabled));
            }
        }

        public string Code
        {
            get => this.code;
            set
            {
                if (this.code == value) return;

                this.code = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.ScriptHash));
                NotifyPropertyChanged(nameof(this.DeployEnabled));
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

                NotifyPropertyChanged();
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

        public ICommand CancelCommand => new RelayCommand(this.TryClose);


        private void Load()
        {
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = "avm",
                Filter = "AVM File|*.avm"
            };

            if (openFileDialog.ShowDialog() != true) return;

            byte[] loadedBytes;
            try
            {
                loadedBytes = File.ReadAllBytes(openFileDialog.FileName);
            }
            catch
            {
                // TODO Show error message
                return;
            }

            this.Code = loadedBytes.ToHexString();
        }

        private void Deploy()
        {
            var transaction = this.GenerateTransaction();

            if (transaction == null) return;

            this.messagePublisher.Publish(new InvokeContractMessage(transaction));
            this.TryClose();
        }

        private InvocationTransaction GenerateTransaction()
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

            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall("Neo.Contract.Create", script, parameterListBytes, returnType, this.NeedsStorage, this.Name, this.Version, this.Author, this.Email, this.Description);
                return new InvocationTransaction
                {
                    Script = builder.ToArray()
                };
            }
        }
    }
}