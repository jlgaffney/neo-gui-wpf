using System.Collections.Generic;
using Neo.Core;

namespace Neo.UI.Core.Controllers.Interfaces
{
    internal interface IBaseBlockchainController
    {
        RegisterTransaction GoverningToken { get; }

        RegisterTransaction UtilityToken { get; }

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true);

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd);
    }
}
