using CommonServiceLocator;
using Neo.Gui.Cross.ViewModels;

namespace Neo.Gui.Cross
{
    internal static class ViewModelLocator
    {
        public static TViewModel GetDataContext<TViewModel>() where TViewModel : ViewModelBase
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
