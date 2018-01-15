using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ImportCertificateViewModel : 
        ViewModelBase, 
        IDialogViewModel<ImportCertificateLoadParameters>,
        ILoadable
    {
        #region Private Fields
        private readonly IStoreCertificateService storeCertificateService;
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
            IStoreCertificateService storeCertificateService,
            IWalletController walletController)
        {
            this.storeCertificateService = storeCertificateService;
            this.walletController = walletController;
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(ImportCertificateLoadParameters parameters)
        {
        }
        #endregion

        #region ILoadable implementation 
        public void OnLoad()
        {
            var storeCertificates = this.storeCertificateService.GetStoreCertificates();
            this.Certificates = new ObservableCollection<X509Certificate2>(storeCertificates);
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