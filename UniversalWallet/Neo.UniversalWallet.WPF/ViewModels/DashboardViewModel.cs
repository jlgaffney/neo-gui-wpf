using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.UniversalWallet.WPF.Messages;

namespace Neo.UniversalWallet.WPF.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private string selectedAsset;

        #region Public Properties 
        public ObservableCollection<string> Assets { get; private set; }

        public string SelectedAsset
        {
            get
            {
                return this.selectedAsset;
            }
            set
            {
                this.selectedAsset = value;
                this.RaisePropertyChanged();
            }
        }

        public RelayCommand<string> AssetSelectionCommand { get; private set; }
        #endregion

        #region Constructor 
        public DashboardViewModel()
        {
            this.Assets = new ObservableCollection<string> { "NEO", "GAS", "RPX" };
            this.SelectedAsset = this.Assets.First();

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
