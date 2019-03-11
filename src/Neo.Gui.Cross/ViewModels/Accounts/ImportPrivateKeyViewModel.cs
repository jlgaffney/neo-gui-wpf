using System.Linq;
using Neo.Gui.Cross.Extensions;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Accounts
{
    public class ImportPrivateKeyViewModel : ViewModelBase
    {
        private readonly IAccountService accountService;

        private string privateKeysWif;

        public ImportPrivateKeyViewModel() { }
        public ImportPrivateKeyViewModel(
            IAccountService accountService)
        {
            this.accountService = accountService;
        }

        public string PrivateKeysWif
        {
            get => privateKeysWif;
            set
            {
                if (Equals(privateKeysWif, value))
                {
                    return;
                }

                privateKeysWif = value;

                this.RaisePropertyChanged();
                
                this.RaisePropertyChanged(nameof(ImportEnabled));
            }
        }

        public bool ImportEnabled => !string.IsNullOrEmpty(PrivateKeysWif);

        public ReactiveCommand ImportCommand => ReactiveCommand.Create(Import);

        public ReactiveCommand CancelCommand => ReactiveCommand.Create(OnClose);

        private void Import()
        {
            if (!ImportEnabled) return;

            if (string.IsNullOrEmpty(PrivateKeysWif))
            {
                return;
            }

            var wifStrings = PrivateKeysWif.ToLines().Where(line => !string.IsNullOrEmpty(line));

            // TODO Validate format of strings

            foreach (var wif in wifStrings)
            {
                var account = accountService.ImportPrivateKey(wif);

                if (account == null)
                {
                    // TODO Inform user
                }
            }

            OnClose();
        }
    }
}
