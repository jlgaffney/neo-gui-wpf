using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Neo.Gui.Base.Extensions;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;

namespace Neo.UI.Accounts
{
    public class ImportPrivateKeyViewModel : ViewModelBase
    {
        private readonly IMessagePublisher messagePublisher;

        public ImportPrivateKeyViewModel(
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;
        }

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

            this.messagePublisher.Publish(new ImportPrivateKeyMessage(this.WifStrings.ToList()));
            this.TryClose();
        }
    }
}