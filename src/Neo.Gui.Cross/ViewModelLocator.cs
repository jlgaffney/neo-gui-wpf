using CommonServiceLocator;
using Neo.Gui.Cross.ViewModels;
using Neo.Gui.Cross.ViewModels.Wallets;

namespace Neo.Gui.Cross
{
    internal static class ViewModelLocator
    {
        public static TViewModel GetDataContext<TViewModel, TLoadParameters>(TLoadParameters parameters)
            where TViewModel : ViewModelBase, ILoadable<TLoadParameters>
        {
            var viewModel = ServiceLocator.Current.GetInstance<TViewModel>();

            viewModel.Load(parameters);

            // TODO Handle IUnloadable somehow (maybe using IDisposable and calling Unload() with disposal method)

            return viewModel;
        }

        public static TViewModel GetDataContext<TViewModel>()
            where TViewModel : ViewModelBase
        {
            var viewModel = ServiceLocator.Current.GetInstance<TViewModel>();

            if (viewModel is ILoadable loadable)
            {
                loadable.Load();
            }

            // TODO Handle IUnloadable somehow (maybe using IDisposable and calling Unload() with disposal method)

            return viewModel;
        }
    }
}
