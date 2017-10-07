using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Neo.Core;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Base.MVVM;
using Neo.UI.Base.Wrappers;
using Neo.UI.Base.Dialogs;
using Neo.UI.Transactions;

namespace Neo.UI.Development
{
    public class TransactionBuilderViewModel : ViewModelBase
    {
        private TransactionType selectedTransactionType;

        private TransactionWrapper transactionWrapper;



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

                NotifyPropertyChanged();

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
                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.SidePanelEnabled));
            }
        }

        public bool SidePanelEnabled => this.TransactionWrapper != null;

        public bool SetupOutputsEnabled => App.CurrentWallet != null;

        public bool FindUnspentCoinsEnabled => App.CurrentWallet != null;

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
            remark = InputBox.Show(Strings.EnterRemarkMessage, Strings.EnterRemarkTitle, remark);
            if (remark != null)
            {
                attribute.Data = Encoding.UTF8.GetBytes(remark);
                if (!found) tx.Attributes.Add(attribute);
            }
        }

        private void SetupOutputs()
        {
            var view = new PayToView();
            view.ShowDialog();

            var output = view.GetOutput();

            if (output == null) return;

            var transaction = (TransactionWrapper) this.TransactionWrapper;
            transaction.Outputs.Add(new TransactionOutputWrapper
            {
                AssetId = (UInt256)output.AssetId,
                Value = new Fixed8((long)output.Value.Value),
                ScriptHash = output.ScriptHash
            });
        }

        private void FindUnspentCoins()
        {
            var wrapper = (TransactionWrapper)this.TransactionWrapper;
            var transaction = App.CurrentWallet.MakeTransaction(wrapper.Unwrap());
            if (transaction == null)
            {
                MessageBox.Show(Strings.InsufficientFunds);
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
            InformationBox.Show(context.ToString(), "ParametersContext", "ParametersContext");
        }
    }
}