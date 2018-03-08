using Autofac;
using GalaSoft.MvvmLight;
using Neo.UniversalWallet.ViewModels.Helpers;
using Neo.UniversalWallet.ViewModels.Helpers.Messages;

namespace Neo.UniversalWallet.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Private fields
        private static ILifetimeScope _containerLifetimeScope;

        private object _pageContent;
        #endregion

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
            this.PageContent = LoadView("LoadWalletView");

            MessengerInstance.Register<NavigationMessage>(this, this.HandleNavigationMessage);
        }

        private void HandleNavigationMessage(NavigationMessage obj)
        {
            this.PageContent = LoadView(obj.DestinationPage);
        }

        public static void SetLifetimeScope(ILifetimeScope lifetimeScope)
        {
            _containerLifetimeScope = lifetimeScope;
        }

        private static object LoadView(object viewName)
        {
            var viewInstance = _containerLifetimeScope.ResolveKeyed<IView>(viewName);
            return viewInstance;
        }
    }
}
