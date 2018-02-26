namespace Neo.UI.Core.Services.Interfaces
{
    public interface IDirectoryManager
    {
        bool DirectoryExists(string path);

        string[] GetSubDirectories(string directoryPath);

        string[] GetFiles(string directoryPath);

        string[] GetDirectoryContents(string directoryPath);

        void Create(string newDirectoryPath);

        void Delete(string directoryPath);

        void Move(string sourceDirPath, string destDirPath);
    }
}
