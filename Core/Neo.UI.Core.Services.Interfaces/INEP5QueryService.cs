using System.Collections.Generic;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Services.Interfaces
{
    public interface INEP5QueryService
    {
        NEP5AssetItem GetTotalBalance(UInt160 nep5ScriptHash, IEnumerable<UInt160> accountScriptHashes);

        IDictionary<UInt160, BigDecimal> GetBalances(UInt160 nep5ScriptHash, IEnumerable<UInt160> accountScriptHashes);
    }
}
