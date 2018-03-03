using System;
using System.IO;
using System.IO.Compression;
using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.IO;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.UI.Core.Internal.Services.Implementations
{
    internal class BlockchainImportService : IBlockchainImportService
    {
        private const string accPath = "chain.acc";
        private const string accZipPath = accPath + ".zip";

        public bool BlocksAreAvailableToImport => File.Exists(accPath) || File.Exists(accZipPath);

        public void ImportBlocks(Blockchain chain)
        {
            if (chain == null) return;

            if (File.Exists(accPath)) // Check if uncompressed import file exists
            {
                // Import blocks
                bool importCompleted;
                using (var fileStream = new FileStream(accPath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    importCompleted = ImportBlocks(chain, fileStream);
                }

                if (importCompleted)
                {
                    File.Delete(accPath);
                }
            }
            else if (File.Exists(accZipPath)) // Check if compressed import file exists
            {
                // Import blocks
                bool importCompleted;
                using (var fileStream = new FileStream(accZipPath, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                using (var zipStream = zip.GetEntry(accPath).Open())
                {
                    importCompleted = ImportBlocks(chain, zipStream);
                }

                if (importCompleted)
                {
                    File.Delete(accZipPath);
                }
            }
        }

        private static bool ImportBlocks(Blockchain chain, Stream importStream)
        {
            var levelDBBlockchain = chain as LevelDBBlockchain;

            if (levelDBBlockchain != null)
            {
                levelDBBlockchain.VerifyBlocks = false;
            }

            using (var reader = new BinaryReader(importStream))
            {
                var count = reader.ReadUInt32();
                for (int height = 0; height < count; height++)
                {
                    var array = reader.ReadBytes(reader.ReadInt32());

                    if (height <= chain.Height) continue;

                    var block = array.AsSerializable<Block>();

                    try
                    {
                        chain.AddBlock(block);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Blockchain instance has been disposed. This is most likely due to the application exiting

                        return false;
                    }
                }
            }

            if (levelDBBlockchain != null)
            {
                levelDBBlockchain.VerifyBlocks = true;
            }

            return true;
        }
    }
}
