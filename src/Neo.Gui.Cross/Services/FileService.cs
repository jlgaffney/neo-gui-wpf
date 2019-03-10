﻿using System.IO;

namespace Neo.Gui.Cross.Services
{
    public class FileService : IFileService
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            if (!FileExists(path))
            {
                return null;
            }

            return File.ReadAllBytes(path);
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            if (!FileExists(path))
            {
                return;
            }

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
    }
}
