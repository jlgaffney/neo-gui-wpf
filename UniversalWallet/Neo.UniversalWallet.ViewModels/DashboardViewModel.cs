using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.UniversalWallet.ViewModels.Helpers.Messages;

namespace Neo.UniversalWallet.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private string _selectedAsset;

        #region Public Properties 
        public ObservableCollection<string> Assets { get; }

        public string SelectedAsset
        {
            get => this._selectedAsset;
            set
            {
                this._selectedAsset = value;
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
