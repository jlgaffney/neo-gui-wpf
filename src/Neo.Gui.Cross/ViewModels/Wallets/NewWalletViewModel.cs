using System.Windows.Input;
using Avalonia.Controls;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Wallets
{
    public class NewWalletViewModel : ViewModelBase
    {
        private readonly ISettings settings;
        private readonly IWalletService walletService;

        private string filePath;
        private string password;
        private string reEnteredPassword;

        public NewWalletViewModel() { }
        public NewWalletViewModel(
            ISettings settings,
            IWalletService walletService)
        {
            this.settings = settings;
            this.walletService = walletService;
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

        public string ReEnteredPassword
        {
            get => reEnteredPassword;
            set
            {
                if (reEnteredPassword == value)
                {
                    return;
                }

                reEnteredPassword = value;

                this.RaisePropertyChanged();
            }
        }

        public ICommand BrowseFileCommand => ReactiveCommand.Create(BrowseFile);

        public ICommand CreateCommand => ReactiveCommand.Create(Create);
        
        private async void BrowseFile()
        {
            var sfd = new SaveFileDialog();
            // TODO Title and Filters

            FilePath = await sfd.ShowAsync(null);
        }

        private void Create()
        {
            if (Password != ReEnteredPassword)
            {
                // TODO Indicate password not confirmed

                return;
            }

            walletService.CreateWallet(FilePath, Password);
            
            settings.LastWalletPath = FilePath;
            //settings.Save();

            OnClose();
        }
    }
}
