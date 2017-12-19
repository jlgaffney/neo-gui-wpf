using System.IO;

namespace Neo.Gui.Base.Managers
{
    public class FileManager : IFileManager
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

        #region Public static helper methods

        /// <summary>
        /// Gets a file path that isn't currently being used
        /// </summary>
        public static string GetAvailableFilePath(string filePath)
        {
            var index = 0;
            while (File.Exists(filePath))
            {
                index++;
                filePath = CreateNumberedFilename(filePath, index);
            }

            return filePath;
        }

        #endregion

        #region Private static helper methods
        
        private static string CreateNumberedFilename(string filename, int number)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            var extension = Path.GetExtension(filename);

            return $"{nameWithoutExtension}{number}{extension}";
        }

        #endregion
    }
}
