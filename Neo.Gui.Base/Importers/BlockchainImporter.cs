using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.IO;

namespace Neo.Gui.Base.Importers
{
    public static class BlockchainImporter
    {
        public static async void ImportBlocksIfRequired(Blockchain blockchain)
        {
            if (blockchain == null) return;

            const string accPath = "chain.acc";
            const string accZipPath = accPath + ".zip";

            await Task.Run(() =>
            {
                if (File.Exists(accPath)) // Check if import file exists
                {
                    // Import blocks
                    bool importCompleted;
                    using (var fileStream = new FileStream(accPath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        importCompleted = ImportBlocks(blockchain, fileStream);
                    }

                    if (importCompleted)
                    {
                        File.Delete(accPath);
                    }
                }
                else if (File.Exists(accZipPath)) // Check if ZIP import file exists
                {
                    // Import blocks
                    bool importCompleted;
                    using (var fileStream = new FileStream(accZipPath, FileMode.Open, FileAccess.Read, FileShare.None))
                    using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                    using (var zipStream = zip.GetEntry(accPath).Open())
                    {
                        importCompleted = ImportBlocks(blockchain, zipStream);
                    }

                    if (importCompleted)
                    {
                        File.Delete(accZipPath);
                    }
                }
            });
        }

        private static bool ImportBlocks(Blockchain blockchain, Stream stream)
        {
            var levelDBBlockchain = blockchain as LevelDBBlockchain;

            if (levelDBBlockchain != null)
            {
                levelDBBlockchain.VerifyBlocks = false;
            }

            using (var reader = new BinaryReader(stream))
            {
                var count = reader.ReadUInt32();
                for (int height = 0; height < count; height++)
                {
                    var array = reader.ReadBytes(reader.ReadInt32());

                    if (height <= blockchain.Height) continue;

                    var block = array.AsSerializable<Block>();

                    try
                    {
                        blockchain.AddBlock(block);
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
