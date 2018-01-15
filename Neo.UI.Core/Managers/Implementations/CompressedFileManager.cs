using System.IO.Compression;
using Neo.UI.Core.Managers.Interfaces;

namespace Neo.UI.Core.Managers.Implementations
{
    internal class CompressedFileManager : ICompressedFileManager
    {
        public void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryPath)
        {
            ZipFile.ExtractToDirectory(sourceZipFilePath, destinationDirectoryPath);
        }
    }
}
