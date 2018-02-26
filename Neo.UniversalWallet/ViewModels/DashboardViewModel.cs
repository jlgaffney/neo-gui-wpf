using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.UniversalWallet.Messages;

namespace Neo.UniversalWallet.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        #region Public Properties 
        public RelayCommand<string> AssetSelectionCommand { get; private set; }
        #endregion

        #region Constructor 
        public DashboardViewModel()
        {
            this.AssetSelectionCommand = new RelayCommand<string>(this.HandleAssetSelection);
        }
        #endregion

        #region Private Methods 
        private void HandleAssetSelection(string assetId)
        {
            MessengerInstance.Send(new NavigationMessage("AssetView"));
        }
        #endregion
    }
}
