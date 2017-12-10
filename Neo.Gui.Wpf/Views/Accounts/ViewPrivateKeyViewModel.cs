using System;
using System.Linq;

using Neo.Wallets;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.MVVM;

using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public class ViewPrivateKeyViewModel : ViewModelBase, IDialogViewModel<ViewPrivateKeyDialogResult>, ILoadable
    {
        private readonly IWalletController walletController;

        public ViewPrivateKeyViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;
        }

        #region Public Properties
        public string Address { get; private set; }

        public string PublicKeyHex { get; private set; }

        public string PrivateKeyHex { get; private set; }

        public string PrivateKeyWif { get; private set; }
        #endregion Public Properties

        public RelayCommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        
        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<ViewPrivateKeyDialogResult> SetDialogResultAndClose;

        public ViewPrivateKeyDialogResult DialogResult { get; private set; }
        #endregion

        #region ILoadable Methods 
        public void OnLoad(params object[] parameters)
        {
            if (!parameters.Any()) return;

            var viewPrivateKeyLoadParameters = (parameters[0] as LoadParameters<ViewPrivateKeyLoadParameters>)?.Parameters;

            if (viewPrivateKeyLoadParameters == null) return;

            this.SetKeyInfo(viewPrivateKeyLoadParameters.Key, viewPrivateKeyLoadParameters.ScriptHash);
        }
        #endregion

        #region Private Methods 

        private void SetKeyInfo(KeyPair key, UInt160 scriptHash)
        {
            this.Address = this.walletController.ToAddress(scriptHash);
            this.PublicKeyHex = key.PublicKey.EncodePoint(true).ToHexString();
            using (key.Decrypt())
            {
                this.PrivateKeyHex = key.PrivateKey.ToHexString();
            }
            this.PrivateKeyWif = key.Export();

            // Update properties
            NotifyPropertyChanged(nameof(this.Address));
            NotifyPropertyChanged(nameof(this.PublicKeyHex));
            NotifyPropertyChanged(nameof(this.PrivateKeyHex));
            NotifyPropertyChanged(nameof(this.PrivateKeyWif));
        }
        
        #endregion
    }
}