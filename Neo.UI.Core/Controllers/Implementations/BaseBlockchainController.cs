using System.Collections.Generic;
using Neo.Core;
using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.UI.Core.Controllers.Implementations
{
    internal abstract class BaseBlockchainController : IBaseBlockchainController
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
