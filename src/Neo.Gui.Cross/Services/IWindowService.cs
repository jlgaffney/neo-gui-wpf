using System.Threading.Tasks;
using Neo.Gui.Cross.ViewModels;

namespace Neo.Gui.Cross.Services
{
    public interface IWindowService
    {
        void Show<TViewModel>() where TViewModel : ViewModelBase;

        Task ShowDialog<TViewModel>() where TViewModel : ViewModelBase;
    }
}
