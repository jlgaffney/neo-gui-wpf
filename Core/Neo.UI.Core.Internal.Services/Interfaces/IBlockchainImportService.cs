using Neo.Core;

namespace Neo.UI.Core.Internal.Services.Interfaces
{
    public interface IBlockchainImportService
    {
        bool BlocksAreAvailableToImport { get; }

        void ImportBlocks(Blockchain chain);
    }
}
