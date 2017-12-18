using Microsoft.Win32;
using Neo.Gui.Base.Services;

namespace Neo.Gui.Wpf.Implementations.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string OpenFileDialog(string filter = null, string defaultExtension = null)
        {
            if (filter == null)
            {
                filter = string.Empty;
            }
            if (defaultExtension == null)
            {
                defaultExtension = string.Empty;
            }

            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = defaultExtension,
                Filter = filter
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        public string SaveFileDialog(string filter = null, string defaultExtension = null)
        {
            if (filter == null)
            {
                filter = string.Empty;
            }
            if (defaultExtension == null)
            {
                defaultExtension = string.Empty;
            }

            var saveFileDialog = new SaveFileDialog
            {
                DefaultExt = defaultExtension,
                Filter = filter
            };

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }
    }
}
