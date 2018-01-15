namespace Neo.UI.Core.Services.Interfaces
{
    public interface IFileDialogService
    {
        string OpenFileDialog(string filter = null, string defaultExtension = null);

        string SaveFileDialog(string filter = null, string defaultExtension = null);
    }
}
