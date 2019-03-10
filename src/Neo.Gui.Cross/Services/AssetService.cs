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

        public AssetState GetAssetState(UInt256 assetId)
        {
            using (var snapshot = blockchainService.GetSnapshot())
            {
                return snapshot.Assets.TryGet(assetId);
            }
        }
    }
}
