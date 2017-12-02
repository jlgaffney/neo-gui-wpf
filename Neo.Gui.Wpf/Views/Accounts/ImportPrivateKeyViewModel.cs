using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Accounts
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