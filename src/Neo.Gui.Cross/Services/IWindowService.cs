using System.Threading.Tasks;
using Neo.Gui.Cross.ViewModels;

namespace Neo.Gui.Cross.Services
{
    public interface IWindowService
    {
        void Show<TViewModel>() where TViewModel : ViewModelBase;

        void Show<TViewModel, TLoadParameters>(TLoadParameters parameters)
            where TViewModel : ViewModelBase, ILoadable<TLoadParameters>;

        Task ShowDialog<TViewModel>() where TViewModel : ViewModelBase;

        Task ShowDialog<TViewModel, TLoadParameters>(TLoadParameters parameters)
            where TViewModel : ViewModelBase, ILoadable<TLoadParameters>;
    }
}
