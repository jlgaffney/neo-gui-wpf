using System.Collections.Generic;
using Neo.Core;

namespace Neo.UI.Core.Services.Interfaces
{
    public interface IBaseBlockchainService
    {
        RegisterTransaction GoverningToken { get; }

        RegisterTransaction UtilityToken { get; }

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true);

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd);
    }
}
