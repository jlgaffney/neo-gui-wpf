using System.IO;

using Neo.Gui.Base.Managers.Interfaces;

namespace Neo.Gui.Base.Managers.Implementations
{
    internal class FileManager : IFileManager
    {
        #region IFileManager implementation

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            if (!this.FileExists(path)) return null;

            return File.ReadAllBytes(path);
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            if (!this.FileExists(path)) return;

            File.WriteAllBytes(path, bytes);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public void Move(string sourceFilePath, string destFilePath)
        {
            File.Move(sourceFilePath, destFilePath);
        }

        #endregion
    }
}
