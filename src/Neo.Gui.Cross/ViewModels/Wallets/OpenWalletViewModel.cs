using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Neo.Gui.Cross.Messages;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Wallets
{
    public class OpenWalletViewModel : ViewModelBase
    {
        private readonly ISettings settings;
        private readonly IWalletService walletService;

        private string filePath;
        private string password;

        public OpenWalletViewModel() { }
        public OpenWalletViewModel(
            ISettings settings,
            IWalletService walletService)
        {
            this.settings = settings;
            this.walletService = walletService;

            FilePath = settings.LastWalletPath;
        }

        public string FilePath
        {
            get => filePath;
            set
            {
                if (filePath == value)
                {
                    return;
                }

                filePath = value;

                this.RaisePropertyChanged();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                if (password == value)
                {
                    return;
                }

                password = value;

                this.RaisePropertyChanged();
            }
        }

        public ICommand BrowseFileCommand => ReactiveCommand.Create(BrowseFile);

        public ICommand OpenCommand => ReactiveCommand.Create(Open);

        private async void BrowseFile()
        {
            var ofd = new OpenFileDialog();
            // TODO Title and Filters

            var filePaths = await ofd.ShowAsync();

            FilePath = filePaths.Single();
        }

        private void Open()
        {
            if (!walletService.OpenWallet(FilePath, Password, out var upgradedWalletPath))
            {
                // TODO Display open unsuccessful message

                return;
            }

            settings.LastWalletPath = upgradedWalletPath ?? FilePath;
            //settings.Save();

            OnClose();
        }
    }
}
