using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.UniversalWallet.WPF.Messages;

namespace Neo.UniversalWallet.WPF.ViewModels
{
    public class LoadWalletViewModel : ViewModelBase
    {
        #region Public Properties 
        public ObservableCollection<string> Networks { get; private set; }

        public string SelectedNetwork { get; set; }

        public RelayCommand UnlockWalletCommand { get; private set; }
        #endregion

        #region Constructor
        public LoadWalletViewModel()
        {
            this.Networks = new ObservableCollection<string> { "Mainnet", "Testnet", "CoZ Testnet", "Privatenet" };
            this.SelectedNetwork = this.Networks.First();

            this.UnlockWalletCommand = new RelayCommand(this.HandleUnlockWallet);
        }
        #endregion

        #region Private Methods 
        private void HandleUnlockWallet()
        {
            MessengerInstance.Send(new NavigationMessage("DashboardView"));
        }
        #endregion
    }
}
