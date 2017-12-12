using System;
using System.Collections.Generic;
using System.Linq;

using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;

using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public class ImportPrivateKeyViewModel : ViewModelBase, IDialogViewModel<ImportPrivateKeyDialogResult>
    {
        #region Private Fields 
        private readonly IMessagePublisher messagePublisher;

        private string privateKeyWif;
        #endregion

        #region Public Properties 
        public bool OkEnabled => !string.IsNullOrEmpty(this.PrivateKeyWif);

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

        public RelayCommand OkCommand => new RelayCommand(this.Ok);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
        public ImportPrivateKeyViewModel(
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<ImportPrivateKeyDialogResult> SetDialogResultAndClose;

        public ImportPrivateKeyDialogResult DialogResult { get; private set; }
        #endregion

        #region Private Methods 
        private void Ok()
        {
            if (!this.OkEnabled) return;

            this.messagePublisher.Publish(new ImportPrivateKeyMessage(this.WifStrings.ToList()));

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}