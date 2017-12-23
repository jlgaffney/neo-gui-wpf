using System;
using System.Collections.Generic;

using Neo.Core;
using Neo.Network;

namespace Neo.Gui.Base.Controllers
{
    public interface IBaseBlockchainController : IDisposable
    {
        RegisterTransaction GoverningToken { get; }

        RegisterTransaction UtilityToken { get; }

        void Initialize();

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true);

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd);

        void Relay(Transaction transaction);

        void Relay(IInventory inventory);
    }
}
