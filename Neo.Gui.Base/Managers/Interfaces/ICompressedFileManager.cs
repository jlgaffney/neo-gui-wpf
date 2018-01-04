namespace Neo.Gui.Base.Managers.Interfaces
{
    public interface ICompressedFileManager
    {
        void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryPath);
    }
}
