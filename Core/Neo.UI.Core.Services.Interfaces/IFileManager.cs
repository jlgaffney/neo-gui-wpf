namespace Neo.UI.Core.Services.Interfaces
{
    public interface IFileManager
    {
        bool FileExists(string path);
    
        byte[] ReadAllBytes(string path);

        void WriteAllBytes(string path, byte[] bytes);

        void Delete(string path);

        void Move(string sourceFilePath, string destFilePath);
    }
}
