using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Controllers.Interfaces;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.MVVM;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ImportCertificateViewModel : 
        ViewModelBase, 
        IDialogViewModel<ImportCertificateDialogResult>,
        ILoadable
    {
        #region Private Fields
        private readonly IWalletController walletController;

        private X509Certificate2 selectedCertificate;
        #endregion

        #region Public Properties 

        public ObservableCollection<X509Certificate2> Certificates { get; private set;  }

        public X509Certificate2 SelectedCertificate
        {
            get => this.selectedCertificate;
            set
            {
                if (Equals(this.selectedCertificate, value)) return;

                this.selectedCertificate = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public bool OkEnabled => this.SelectedCertificate != null;

        public RelayCommand OkCommand => new RelayCommand(this.Ok);
        #endregion

        #region Constructor 
        public ImportCertificateViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<ImportCertificateDialogResult> SetDialogResultAndClose;

        public ImportCertificateDialogResult DialogResult { get; private set; }
        #endregion

        #region ILoadable implementation 
        public void OnLoad()
        {
            // Load certificates
            using (var store = new X509Store())
            {
                store.Open(OpenFlags.ReadOnly);

                this.Certificates = new ObservableCollection<X509Certificate2>(
                    store.Certificates.Cast<X509Certificate2>());
            }
        }
        #endregion

        #region Private Methods 
        private void Ok()
        {
            if (this.SelectedCertificate == null) return;

            this.walletController.ImportCertificate(this.SelectedCertificate);

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}