namespace Neo.Gui.Cross.Services
{
    public interface IFileService
    {
        bool FileExists(string path);

        byte[] ReadAllBytes(string path);

        void WriteAllBytes(string path, byte[] bytes);

        void Delete(string path);

        void Move(string sourceFilePath, string destFilePath);
    }
}
