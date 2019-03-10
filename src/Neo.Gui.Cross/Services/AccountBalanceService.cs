using System.Collections.Generic;
using System.Numerics;
using Neo.Ledger;

namespace Neo.Gui.Cross.Services
{
    public class AccountBalanceService : IAccountBalanceService
    {
        private readonly Dictionary<UInt160, Dictionary<UInt256, Fixed8>> globalAssetBalances
            = new Dictionary<UInt160, Dictionary<UInt256, Fixed8>>();

        // TODO Determine appropriate type to store NEP-5 balance (e.g. tuple of BigInteger and integer for decimal places?)
        private readonly Dictionary<UInt160, Dictionary<UInt160, BigDecimal>> nep5TokenBalances
            = new Dictionary<UInt160, Dictionary<UInt160, BigDecimal>>();

        public bool GlobalAssetBalanceChanged { get; set; }

        public bool NEP5TokenBalanceChanged { get; set; }



        public IReadOnlyDictionary<UInt256, Fixed8> GetGlobalAssetBalances(UInt160 accountId)
        {
            if (!globalAssetBalances.ContainsKey(accountId))
            {
                return new Dictionary<UInt256, Fixed8>()
                /*{
                    { Blockchain.GoverningToken.Hash, Fixed8.FromDecimal(123) },
                    { Blockchain.UtilityToken.Hash, Fixed8.FromDecimal(1234) }
                }*/;
            }

            return new Dictionary<UInt256, Fixed8>(globalAssetBalances[accountId])
            /*{
                { Blockchain.GoverningToken.Hash, Fixed8.FromDecimal(123) },
                { Blockchain.UtilityToken.Hash, Fixed8.FromDecimal(1234) }
            }*/;
        }

        public IReadOnlyDictionary<UInt160, BigDecimal> GetNEP5TokenBalances(UInt160 accountId)
        {
            if (!nep5TokenBalances.ContainsKey(accountId))
            {
                return new Dictionary<UInt160, BigDecimal>();
            }

            return new Dictionary<UInt160, BigDecimal>(nep5TokenBalances[accountId]);
        }

        public void UpdateGlobalAssetBalance(UInt160 accountId, UInt256 assetId, Fixed8 balance)
        {
            if (!globalAssetBalances.ContainsKey(accountId))
            {
                globalAssetBalances.Add(accountId, new Dictionary<UInt256, Fixed8>());
            }

            var accountBalances = globalAssetBalances[accountId];

            if (balance <= Fixed8.Zero)
            {
                if (accountBalances.ContainsKey(assetId))
                {
                    accountBalances.Remove(assetId);
                }

                return;
            }

            accountBalances[assetId] = balance;
        }

        public void UpdateNEP5TokenBalance(UInt160 accountId, UInt160 nep5ScriptHash, BigDecimal balance)
        {
            if (!nep5TokenBalances.ContainsKey(accountId))
            {
                nep5TokenBalances.Add(accountId, new Dictionary<UInt160, BigDecimal>());
            }

            var accountBalances = nep5TokenBalances[accountId];

            if (balance.Value <= BigInteger.Zero)
            {
                if (accountBalances.ContainsKey(nep5ScriptHash))
                {
                    accountBalances.Remove(nep5ScriptHash);
                }

                return;
            }

            accountBalances[nep5ScriptHash] = balance;
        }



        public void Clear()
        {
            globalAssetBalances.Clear();
            nep5TokenBalances.Clear();
        }
    }
}
