using System.Threading.Tasks;

namespace Neo.Gui.Cross.Services
{
    // TODO Add file filter support
    public interface IFileDialogService
    {
        Task<string> OpenFileDialog();

        Task<string> SaveFileDialog();
    }
}
