using System.IO;
using System.Linq;
using Neo.UI.Core.Managers.Interfaces;

namespace Neo.UI.Core.Managers.Implementations
{
    internal class DirectoryManager : IDirectoryManager
    {
        #region IDirectoryManager implementation

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] GetSubDirectories(string directoryPath)
        {
            return GetDirectoryContents(directoryPath, FileSystemInfoType.Directory);
        }

        public string[] GetFiles(string directoryPath)
        {
            return GetDirectoryContents(directoryPath, FileSystemInfoType.File);
        }

        public string[] GetDirectoryContents(string directoryPath)
        {
            return GetDirectoryContents(directoryPath, FileSystemInfoType.Any);
        }

        public void Create(string newDirectoryPath)
        {
            if (this.DirectoryExists(newDirectoryPath)) return;

            Directory.CreateDirectory(newDirectoryPath);
        }

        public void Delete(string directoryPath)
        {
            Directory.Delete(directoryPath);
        }

        public void Move(string sourceDirPath, string destDirPath)
        {
            Directory.Move(sourceDirPath, destDirPath);
        }

        #endregion

        #region Private methods

        private static string[] GetDirectoryContents(string directoryPath, FileSystemInfoType type)
        {
            if (!Directory.Exists(directoryPath)) return null;

            var fileSystemInfo = new DirectoryInfo(directoryPath).GetFileSystemInfos();

            return fileSystemInfo.Where(info =>
            {
                switch (type)
                {
                    case FileSystemInfoType.File:
                        return info is FileInfo;

                    case FileSystemInfoType.Directory:
                        return info is DirectoryInfo;
                }

                return true;
            }).Select(info => info.FullName).ToArray();
        }

        #endregion

        private enum FileSystemInfoType
        {
            File,
            Directory,
            Any
        }
    }
}
