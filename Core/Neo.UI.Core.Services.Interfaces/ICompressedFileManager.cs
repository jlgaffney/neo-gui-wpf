namespace Neo.UI.Core.Services.Interfaces
{
    public interface ICompressedFileManager
    {
        void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryPath);
    }
}
