using Neo.Cryptography;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Accounts
{
    public class ViewPrivateKeyViewModel :
        ViewModelBase,
        ILoadable<UInt160>
    {
        private readonly IAccountService accountService;

        private string address;
        private string publicKeyHex;
        private string privateKeyHex;
        private string privateKeyWif;

        public ViewPrivateKeyViewModel() { }
        public ViewPrivateKeyViewModel(
            IAccountService accountService)
        {
            this.accountService = accountService;
        }

        public string Address
        {
            get => address;
            private set
            {
                if (Equals(address, value))
                {
                    return;
                }

                address = value;

                this.RaisePropertyChanged();
            }
        }

        public string PublicKeyHex
        {
            get => publicKeyHex;
            private set
            {
                if (Equals(publicKeyHex, value))
                {
                    return;
                }

                publicKeyHex = value;

                this.RaisePropertyChanged();
            }
        }

        public string PrivateKeyHex
        {
            get => privateKeyHex;
            private set
            {
                if (Equals(privateKeyHex, value))
                {
                    return;
                }

                privateKeyHex = value;

                this.RaisePropertyChanged();
            }
        }

        public string PrivateKeyWif
        {
            get => privateKeyWif;
            private set
            {
                if (Equals(privateKeyWif, value))
                {
                    return;
                }

                privateKeyWif = value;

                this.RaisePropertyChanged();
            }
        }

        public ReactiveCommand CloseCommand => ReactiveCommand.Create(OnClose);
        
        public void Load(UInt160 accountScriptHash)
        {
            if (accountScriptHash == null)
            {
                return;
            }

            var account = accountService.GetAccount(accountScriptHash);

            if (account == null)
            {
                // TODO Inform user somehow
                return;
            }

            var accountKey = account.GetKey();

            Address = account.Address;
            PublicKeyHex = accountKey.PublicKey.ToString();
            PrivateKeyHex = accountKey.PrivateKey.ToHexString();
            PrivateKeyWif = accountKey.PrivateKey.Base58CheckEncode();
        }
    }
}
