using System.IO.Compression;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.UI.Core.Services.Implementations
{
    internal class CompressedFileManager : ICompressedFileManager
    {
        public void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryPath)
        {
            ZipFile.ExtractToDirectory(sourceZipFilePath, destinationDirectoryPath);
        }
    }
}
