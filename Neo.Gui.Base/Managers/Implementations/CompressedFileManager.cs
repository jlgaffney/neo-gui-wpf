using System.IO.Compression;

using Neo.Gui.Base.Managers.Interfaces;

namespace Neo.Gui.Base.Managers.Implementations
{
    internal class CompressedFileManager : ICompressedFileManager
    {
        public void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryPath)
        {
            ZipFile.ExtractToDirectory(sourceZipFilePath, destinationDirectoryPath);
        }
    }
}
