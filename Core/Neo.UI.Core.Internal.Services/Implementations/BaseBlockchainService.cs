using Neo.Core;

namespace Neo.UI.Core.Internal.Services.Implementations
{
    internal abstract class BaseBlockchainService
    {
        #region IBaseBlockchainController implementation

        public UInt256 GoverningTokenHash => Blockchain.GoverningToken.Hash;

        public UInt256 UtilityTokenHash => Blockchain.UtilityToken.Hash;

        #endregion
    }
}
