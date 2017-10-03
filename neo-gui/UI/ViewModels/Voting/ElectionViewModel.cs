using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.SmartContract;
using Neo.UI.MVVM;
using Neo.VM;

namespace Neo.UI.ViewModels.Voting
{
    public class ElectionViewModel : ViewModelBase
    {
        private ECPoint selectedBookKeeper;

        public ElectionViewModel()
        {
            this.BookKeepers = new ObservableCollection<ECPoint>();

            if (App.CurrentWallet == null) return;
            
            // Load book keepers
            foreach (var bookKeeper in App.CurrentWallet.GetContracts().Where(p => p.IsStandard).Select(p =>
                App.CurrentWallet.GetKey(p.PublicKeyHash).PublicKey))
            {
                this.BookKeepers.Add(bookKeeper);
            }
        }

        public ObservableCollection<ECPoint> BookKeepers { get; }

        public ECPoint SelectedBookKeeper
        {
            get => this.selectedBookKeeper;
            set
            {
                if (this.selectedBookKeeper != null && this.selectedBookKeeper.Equals(value)) return;

                this.selectedBookKeeper = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }

        public InvocationTransaction Transaction { get; private set; }

        public bool OkEnabled => this.SelectedBookKeeper != null;
        
        public ICommand OkCommand => new RelayCommand(this.Ok);

        private void Ok()
        {
            var transaction = this.GetTransaction();

            if (transaction == null) return;

            this.Transaction = transaction;

            this.TryClose();
        }

        private InvocationTransaction GetTransaction()
        {
            if (this.SelectedBookKeeper == null) return null;

            var publicKey = this.SelectedBookKeeper;

            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall("Neo.Validator.Register", publicKey);
                return new InvocationTransaction
                {
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = Contract.CreateSignatureRedeemScript(publicKey).ToScriptHash().ToArray()
                        }
                    },
                    Script = builder.ToArray()
                };
            }
        }
    }
}