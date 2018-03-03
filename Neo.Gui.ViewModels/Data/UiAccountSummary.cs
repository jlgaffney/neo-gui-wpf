using GalaSoft.MvvmLight;
using Neo.UI.Core.Data.Enums;

namespace Neo.Gui.ViewModels.Data
{
    public class UiAccountSummary : ObservableObject
    {
        #region Private Fields 
        private int neo;
        private decimal gas;
        #endregion

        #region Public Properties 
        public string Label { get; }

        public string Address { get; }

        public string ScriptHash { get; }

        public AccountType Type { get; }

        public int Neo
        {
            get => this.neo;
            set
            {
                if (this.neo == value) return;

                this.neo = value;

                RaisePropertyChanged();
            }
        }

        public decimal Gas
        {
            get => this.gas;
            set
            {
                if (this.gas == value) return;

                this.gas = value;

                RaisePropertyChanged();
            }
        }
        #endregion

        #region Constructor 
        public UiAccountSummary(string label, string address, string scriptHash, AccountType accountType)
        {
            this.Label = label;
            this.Address = address;
            this.ScriptHash = scriptHash;
            this.Type = accountType;
        }
        #endregion
    }
}
