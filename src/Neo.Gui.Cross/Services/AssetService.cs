using Neo.Ledger;

namespace Neo.Gui.Cross.Services
{
    public class AssetService : IAssetService
    {
        private readonly IBlockchainService blockchainService;

        public AssetService(
            IBlockchainService blockchainService)
        {
            this.blockchainService = blockchainService;
        }
    }
}
