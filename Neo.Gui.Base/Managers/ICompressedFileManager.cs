namespace Neo.Gui.Base.Managers
{
    public interface ICompressedFileManager
    {
        void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryPath);
    }
}
