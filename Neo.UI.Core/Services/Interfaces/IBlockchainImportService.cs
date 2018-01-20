using Neo.Core;

namespace Neo.UI.Core.Services.Interfaces
{
    internal interface IBlockchainImportService
    {
        bool BlocksAreAvailableToImport { get; }

        void ImportBlocks(Blockchain chain);
    }
}
