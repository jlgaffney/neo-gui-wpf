using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.SmartContract;

using Neo.Gui.Globalization.Resources;
using Neo.Gui.Dialogs.LoadParameters.Transactions;
using Neo.Gui.Dialogs.Results.Transactions;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.Wpf.Wrappers;
using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.Gui.Wpf.Views.Development
{
    public class TransactionBuilderViewModel : ViewModelBase
    {
        private readonly IDialogManager dialogManager;
        private readonly IWalletController walletController;

        private TransactionType selectedTransactionType;
        private TransactionWrapper transactionWrapper;

        public TransactionBuilderViewModel(
            IDialogManager dialogManager,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.walletController = walletController;
        }

        public TransactionType[] TransactionTypes => new[]
        {
            TransactionType.ContractTransaction,
            TransactionType.ClaimTransaction,
            TransactionType.IssueTransaction,
            TransactionType.InvocationTransaction
        };

        public TransactionType SelectedTransactionType
        {
            get => this.selectedTransactionType;
            set
            {
                if (this.selectedTransactionType == value) return;

                this.selectedTransactionType = value;

                RaisePropertyChanged();

                // Transaction type has changed, get new transaction wrapper
                var newTransactionWrapper = GetNewTransactionWrapper();

                this.TransactionWrapper = newTransactionWrapper;
            }
        }

        public object TransactionWrapper
        {
            get => this.transactionWrapper;
            set
            {
                if (this.transactionWrapper == value) return;

                Debug.Assert(value is TransactionWrapper);

                this.transactionWrapper = (TransactionWrapper)value;

                //(TransactionWrapper)propertyGrid1.SelectedObject;
                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.SidePanelEnabled));
            }
        }

        public bool SidePanelEnabled => this.TransactionWrapper != null;

        public bool SetupOutputsEnabled => this.walletController.WalletIsOpen;

        public bool FindUnspentCoinsEnabled => this.walletController.WalletIsOpen;

        #region Commands

        public ICommand TransactionRemarkCommand => new RelayCommand(this.TransactionRemark);

        public ICommand SetupOutputsCommand => new RelayCommand(this.SetupOutputs);

        public ICommand FindUnspentCoinsCommand => new RelayCommand(this.FindUnspentCoins);

        public ICommand GetParametersContextCommand => new RelayCommand(this.GetParametersContext);

        #endregion Commands

        // TODO Second tab of developer tools

        private TransactionWrapper GetNewTransactionWrapper()
        {
            var typeName = $"{typeof(TransactionWrapper).Namespace}.{this.SelectedTransactionType}Wrapper";

            var instance = Assembly.GetExecutingAssembly().CreateInstance(typeName);

            Debug.Assert(instance is TransactionWrapper);

            return (TransactionWrapper)instance;
        }

        private void TransactionRemark()
        {
            var tx = (TransactionWrapper)this.TransactionWrapper;
            var attribute = tx.Attributes.FirstOrDefault(p => p.Usage == TransactionAttributeUsage.Remark);
            var found = attribute != null;
            if (!found)
            {
                attribute = new TransactionAttributeWrapper
                {
                    Usage = TransactionAttributeUsage.Remark,
                    Data = new byte[0]
                };
            }
            var remark = Encoding.UTF8.GetString(attribute.Data);

            var result = this.dialogManager.ShowInputDialog(Strings.EnterRemarkTitle, Strings.EnterRemarkMessage, remark);

            if (!string.IsNullOrEmpty(result))
            {
                remark = result;
            }

            if (!string.IsNullOrEmpty(remark))
            {
                attribute.Data = Encoding.UTF8.GetBytes(remark);
                if (!found) tx.Attributes.Add(attribute);
            }
        }

        private void SetupOutputs()
        {
            var result = dialogManager.ShowDialog<PayToLoadParameters, PayToDialogResult>();

            if (result.Output == null) return;

            var transaction = (TransactionWrapper) this.TransactionWrapper;
            transaction.Outputs.Add(new TransactionOutputWrapper
            {
                AssetId = (UInt256)result.Output.AssetId,
                Value = new Fixed8((long)result.Output.Value.Value),
                ScriptHash = result.Output.ScriptHash
            });
        }

        private void FindUnspentCoins()
        {
            var wrapper = (TransactionWrapper)this.TransactionWrapper;
            var transaction = this.walletController.MakeTransaction(wrapper.Unwrap());
            if (transaction == null)
            {
                this.dialogManager.ShowMessageDialog(string.Empty, Strings.InsufficientFunds);
            }
            else
            {
                wrapper.Inputs = transaction.Inputs.Select(CoinReferenceWrapper.Wrap).ToList();
                wrapper.Outputs = transaction.Outputs.Select(TransactionOutputWrapper.Wrap).ToList();
            }
        }

        private void GetParametersContext()
        {
            var wrapper = (TransactionWrapper)this.TransactionWrapper;
            var context = new ContractParametersContext(wrapper.Unwrap());

            this.dialogManager.ShowInformationDialog("ParametersContext", "ParametersContext", context.ToString());
        }
    }
}