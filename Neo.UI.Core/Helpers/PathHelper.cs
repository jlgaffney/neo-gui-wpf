using System.IO;

namespace Neo.UI.Core.Helpers
{
    internal static class PathHelper
    {
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

        private static string CreateNumberedFilename(string filename, int number)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            var extension = Path.GetExtension(filename);

            return $"{nameWithoutExtension}{number}{extension}";
        }
    }
}
