using System.Collections.Generic;
using System.Windows.Input;
using Neo.Extensions;
using Neo.UI.MVVM;

namespace Neo.UI.ViewModels.Accounts
{
    public class ImportPrivateKeyViewModel : ViewModelBase
    {
        private string privateKeyWif;

        public string PrivateKeyWif
        {
            get => this.privateKeyWif;
            set
            {
                if (this.privateKeyWif == value) return;

                this.privateKeyWif = value;
                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }

        public IEnumerable<string> WifStrings
        {
            get
            {
                if (string.IsNullOrEmpty(this.PrivateKeyWif)) return new string[0];

                return this.PrivateKeyWif.ToLines();
            }
        }

        public bool OkEnabled => !string.IsNullOrEmpty(this.PrivateKeyWif);

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(this.TryClose);

        private void Ok()
        {
            if (!this.OkEnabled) return;

            this.TryClose();
        }
    }
}