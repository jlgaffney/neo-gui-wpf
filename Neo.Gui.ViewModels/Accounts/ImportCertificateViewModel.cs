using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Controllers.Interfaces;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ImportCertificateViewModel : ViewModelBase, IDialogViewModel<ImportCertificateDialogResult>
    {
        private readonly IWalletController walletController;

        private X509Certificate2 selectedCertificate;

        public ImportCertificateViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;

            // Load certificates
            using (var store = new X509Store())
            {
                store.Open(OpenFlags.ReadOnly);

                this.Certificates = new ObservableCollection<X509Certificate2>(
                    store.Certificates.Cast<X509Certificate2>());
            }
        }

        public ObservableCollection<X509Certificate2> Certificates { get; }

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

        public ICommand OkCommand => new RelayCommand(this.Ok);

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<ImportCertificateDialogResult> SetDialogResultAndClose;

        public ImportCertificateDialogResult DialogResult { get; private set; }
        #endregion

        private void Ok()
        {
            if (this.SelectedCertificate == null) return;

            this.walletController.ImportCertificate(this.SelectedCertificate);

            this.Close(this, EventArgs.Empty);
        }
    }
}