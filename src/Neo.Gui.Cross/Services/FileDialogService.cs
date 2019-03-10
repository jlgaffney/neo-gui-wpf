using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Neo.Gui.Cross.Services
{
    public class FileDialogService : IFileDialogService
    {
        public async Task<string> OpenFileDialog()
        {
            var ofd = new OpenFileDialog();

            ofd.AllowMultiple = false;

            // TODO Title and Filters

            var filePaths = await ofd.ShowAsync();

            return filePaths.Single();
        }

        public async Task<string> SaveFileDialog()
        {
            var sfd = new SaveFileDialog();

            // TODO Title and Filters

            return await sfd.ShowAsync(null);
        }
    }
}
