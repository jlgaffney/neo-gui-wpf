using System.Collections.Generic;
using Neo.Core;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.UI.Core.Services.Implementations
{
    internal abstract class BaseBlockchainService : IBaseBlockchainService
    {
        #region IBaseBlockchainController implementation

        public RegisterTransaction GoverningToken => Blockchain.GoverningToken;

        public RegisterTransaction UtilityToken => Blockchain.UtilityToken;

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            return Blockchain.CalculateBonus(inputs, ignoreClaimed);
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd)
        {
            return Blockchain.CalculateBonus(inputs, heightEnd);
        }

        #endregion
    }
}
