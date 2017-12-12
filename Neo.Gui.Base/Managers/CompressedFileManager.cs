using System.IO.Compression;

namespace Neo.Gui.Base.Managers
{
    public class CompressedFileManager : ICompressedFileManager
    {
        public void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryPath)
        {
            ZipFile.ExtractToDirectory(sourceZipFilePath, destinationDirectoryPath);
        }
    }
}
