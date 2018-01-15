using System;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ViewPrivateKeyViewModel : ViewModelBase,
        IDialogViewModel<ViewPrivateKeyLoadParameters>
    {
        #region Private fields

        private readonly IWalletController walletController;

        #endregion

        #region Constructor

        public ViewPrivateKeyViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;
        }
        
        #endregion

        #region Public Properties
        public string Address { get; private set; }

        public string PublicKeyHex { get; private set; }

        public string PrivateKeyHex { get; private set; }

        public string PrivateKeyWif { get; private set; }
        #endregion Public Properties

        public ICommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        
        #region ILoadableDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(ViewPrivateKeyLoadParameters parameters)
        {
            if (parameters == null || parameters.ScriptHash == null) return;

            var key = this.walletController.GetAccountKey(parameters.ScriptHash);

            this.Address = this.walletController.ScriptHashToAddress(parameters.ScriptHash);
            this.PublicKeyHex = key.PublicKey.EncodePoint(true).ToHexString();
            using (key.Decrypt())
            {
                this.PrivateKeyHex = key.PrivateKey.ToHexString();
            }
            this.PrivateKeyWif = key.Export();

            // Update properties
            RaisePropertyChanged(nameof(this.Address));
            RaisePropertyChanged(nameof(this.PublicKeyHex));
            RaisePropertyChanged(nameof(this.PrivateKeyHex));
            RaisePropertyChanged(nameof(this.PrivateKeyWif));
        }
        #endregion
    }
}
