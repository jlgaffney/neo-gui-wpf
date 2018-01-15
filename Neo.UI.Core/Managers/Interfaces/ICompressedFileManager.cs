namespace Neo.UI.Core.Managers.Interfaces
{
    public interface ICompressedFileManager
    {
        void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryPath);
    }
}
