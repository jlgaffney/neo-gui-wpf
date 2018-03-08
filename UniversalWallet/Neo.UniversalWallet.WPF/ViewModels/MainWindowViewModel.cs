using GalaSoft.MvvmLight;
using Neo.UniversalWallet.WPF.Messages;
using Neo.UniversalWallet.WPF.Views;
using AssetView = Neo.UniversalWallet.WPF.Views.AssetView;

namespace Neo.UniversalWallet.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object _pageContent;

        public object PageContent
        {
            get => this._pageContent;
            set
            {
                this._pageContent = value;
                this.RaisePropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            this.PageContent = new LoadWalletView();

            MessengerInstance.Register<NavigationMessage>(this, this.HandleNavigationMessage);
        }

        private void HandleNavigationMessage(NavigationMessage obj)
        {
            if (obj.DestinationPage == "DashboardView")
            {
                this.PageContent = new DashboardView();
            }
            else if(obj.DestinationPage == "AssetView")
            {
                this.PageContent = new AssetView();
            }
        }
    }
}
