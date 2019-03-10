using System.Collections.Generic;

namespace Neo.Gui.Cross.Services
{
    public interface IAccountBalanceService
    {
        bool GlobalAssetBalanceChanged { get; set; }

        bool NEP5TokenBalanceChanged { get; set; }



        IReadOnlyDictionary<UInt256, Fixed8> GetGlobalAssetBalances(UInt160 accountId);

        IReadOnlyDictionary<UInt160, BigDecimal> GetNEP5TokenBalances(UInt160 accountId);

        void UpdateGlobalAssetBalance(UInt160 accountId, UInt256 assetId, Fixed8 balance);

        void UpdateNEP5TokenBalance(UInt160 accountId, UInt160 nep5ScriptHash, BigDecimal balance);


        void Clear();
    }
}
