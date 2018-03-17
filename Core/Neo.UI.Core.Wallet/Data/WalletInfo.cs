using System;
using System.Collections.Generic;
using System.Numerics;

namespace Neo.UI.Core.Wallet.Data
{
    internal class WalletInfo
    {

        private readonly IDictionary<UInt256, Fixed8> assetTotalBalances;
        private readonly IDictionary<UInt160, BigDecimal> nep5TokenTotalBalances;
        
        private readonly IDictionary<UInt256, TransactionMetadata> transactions;

        public WalletInfo()
        {
            this.assetTotalBalances = new Dictionary<UInt256, Fixed8>();
            this.nep5TokenTotalBalances = new Dictionary<UInt160, BigDecimal>();
            
            this.transactions = new Dictionary<UInt256, TransactionMetadata>();
        }

        #region Balances

        public IEnumerable<UInt256> GetAssetsInWallet()
        {
            return this.assetTotalBalances.Keys;
        }

        public Fixed8 GetAssetTotalBalance(UInt256 assetId)
        {
            return this.assetTotalBalances[assetId];
        }

        public bool WalletContainsAsset(UInt256 assetId)
        {
            return this.assetTotalBalances.ContainsKey(assetId);
        }

        public void AddAssetToList(UInt256 assetId, Fixed8 totalBalance)
        {
            if (this.assetTotalBalances.ContainsKey(assetId))
            {
                throw new InvalidOperationException("Asset has already been added!");
            }

            this.assetTotalBalances.Add(assetId, totalBalance);
        }

        public void UpdateAssetTotalBalance(UInt256 assetId, Fixed8 updatedTotalBalance)
        {
            if (!this.assetTotalBalances.ContainsKey(assetId))
            {
                throw new InvalidOperationException("Asset has not been added, balance cannot be updated!");
            }

            this.assetTotalBalances[assetId] = updatedTotalBalance;
        }

        public void RemoveAssetFromList(UInt256 assetId)
        {
            this.assetTotalBalances.Remove(assetId);
        }

        public IEnumerable<UInt160> GetNEP5TokensInWallet()
        {
            return this.nep5TokenTotalBalances.Keys;
        }

        public bool WalletContainsNEP5Token(UInt160 scriptHash)
        {
            return this.nep5TokenTotalBalances.ContainsKey(scriptHash);
        }

        public BigDecimal GetNEP5TokenTotalBalance(UInt160 scriptHash)
        {
            return this.nep5TokenTotalBalances[scriptHash];
        }

        public void AddNEP5TokenToList(UInt160 scriptHash, BigInteger totalBalance, byte decimals)
        {
            if (this.nep5TokenTotalBalances.ContainsKey(scriptHash))
            {
                throw new InvalidOperationException("NEP-5 token has already been added!");
            }

            this.nep5TokenTotalBalances.Add(scriptHash, new BigDecimal(totalBalance, decimals));
        }

        public void UpdateNEP5TokenTotalBalance(UInt160 scriptHash, BigInteger updatedTotalBalance)
        {
            if (!this.nep5TokenTotalBalances.ContainsKey(scriptHash))
            {
                throw new InvalidOperationException("NEP-5 token has not been added, balance cannot be updated!");
            }

            var previousTotalBalance = this.nep5TokenTotalBalances[scriptHash];
            
            this.nep5TokenTotalBalances[scriptHash] = new BigDecimal(updatedTotalBalance, previousTotalBalance.Decimals);
        }

        public void RemoveNEP5TokenFromList(UInt160 scriptHash)
        {
            this.nep5TokenTotalBalances.Remove(scriptHash);
        }

        #endregion

        #region Transactions

        public TransactionMetadata GetTransaction(UInt256 hash)
        {
            if (hash == null) return null;

            if (!this.transactions.ContainsKey(hash)) return null;

            return this.transactions[hash];
        }

        public void AddTransaction(TransactionMetadata transaction)
        {
            if (this.transactions.ContainsKey(transaction.Id))
            {
                throw new InvalidOperationException("Transaction has already been added!");
            }

            this.transactions.Add(transaction.Id, transaction);
        }

        #endregion
    }
}
