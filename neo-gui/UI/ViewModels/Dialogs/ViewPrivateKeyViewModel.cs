using System.Windows.Input;
using Neo.UI.MVVM;
using Neo.Wallets;

namespace Neo.UI.ViewModels.Dialogs
{
    public class ViewPrivateKeyViewModel : ViewModelBase
    {
        public string Address { get; private set; }
        public string PublicKeyHex { get; private set; }

        public string PrivateKeyHex { get; private set; }
        public string PrivateKeyWif { get; private set; }

        public ICommand CloseCommand => new RelayCommand(this.TryClose);

        public void SetKeyInfo(KeyPair key, UInt160 scriptHash)
        {
            this.Address = Wallet.ToAddress(scriptHash);
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
    }
}