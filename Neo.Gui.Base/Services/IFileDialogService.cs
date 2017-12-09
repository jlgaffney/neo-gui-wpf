namespace Neo.Gui.Base.Services
{
    public interface IFileDialogService
    {
        string OpenFileDialog(string defaultExtension = null, string filter = null);

        string SaveFileDialog(string defaultExtension = null, string filter = null);
    }
}
