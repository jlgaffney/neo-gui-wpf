using System;
using System.Collections.Generic;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Wallet.Data
{
    internal class WalletInfo
    {
        private readonly IDictionary<UInt160, AccountSummary> accounts;
        private readonly IDictionary<UInt256, FirstClassAssetSummary> firstClassAssets;
        private readonly IDictionary<UInt160, NEP5AssetSummary> nep5Assets;
        private readonly IDictionary<UInt256, TransactionItem> transactions;

        public WalletInfo()
        {
            this.accounts = new Dictionary<UInt160, AccountSummary>();
            this.firstClassAssets = new Dictionary<UInt256, FirstClassAssetSummary>();
            this.nep5Assets = new Dictionary<UInt160, NEP5AssetSummary>();
            this.transactions = new Dictionary<UInt256, TransactionItem>();
        }

        #region Accounts

        public IEnumerable<AccountSummary> GetAccounts()
        {
            return this.accounts.Values;
        }

        public AccountSummary GetAccount(UInt160 scriptHash)
        {
            if (scriptHash == null) return null;

            if (!this.accounts.ContainsKey(scriptHash)) return null;

            return this.accounts[scriptHash];
        }

        public void AddAccount(AccountSummary account)
        {
            this.accounts.Add(account.ScriptHash, account);
        }

        public void RemoveAccount(UInt160 scriptHash)
        {
            this.accounts.Remove(scriptHash);
        }

        #endregion

        #region Assets

        public IEnumerable<FirstClassAssetSummary> GetFirstClassAssets()
        {
            return this.firstClassAssets.Values;
        }

        public IEnumerable<NEP5AssetSummary> GetNEP5Assets()
        {
            return this.nep5Assets.Values;
        }

        public FirstClassAssetSummary GetFirstClassAsset(UInt256 assetId)
        {
            if (assetId == null) return null;

            if (!this.firstClassAssets.ContainsKey(assetId)) return null;

            return this.firstClassAssets[assetId];
        }

        public NEP5AssetSummary GetNEP5Asset(UInt160 scriptHash)
        {
            if (scriptHash == null) return null;

            if (!this.nep5Assets.ContainsKey(scriptHash)) return null;

            return this.nep5Assets[scriptHash];
        }

        public void AddFirstClassAsset(FirstClassAssetSummary asset)
        {
            if (this.firstClassAssets.ContainsKey(asset.AssetId))
            {
                throw new InvalidOperationException("Asset has already been added!");
            }

            this.firstClassAssets.Add(asset.AssetId, asset);
        }

        public void AddNEP5Asset(NEP5AssetSummary asset)
        {
            if (this.nep5Assets.ContainsKey(asset.ScriptHash))
            {
                throw new InvalidOperationException("Asset has already been added!");
            }

            this.nep5Assets.Add(asset.ScriptHash, asset);
        }

        public void RemoveFirstClassAsset(UInt256 assetId)
        {
            this.firstClassAssets.Remove(assetId);
        }

        public void RemoveNEP5Asset(UInt160 scriptHash)
        {
            this.nep5Assets.Remove(scriptHash);
        }

        #endregion

        #region Transactions

        public TransactionItem GetTransaction(UInt256 hash)
        {
            if (hash == null) return null;

            if (!this.transactions.ContainsKey(hash)) return null;

            return this.transactions[hash];
        }

        public void AddTransaction(TransactionItem transaction)
        {
            if (this.transactions.ContainsKey(transaction.Hash))
            {
                throw new InvalidOperationException("Transaction has already been added!");
            }

            this.transactions.Add(transaction.Hash, transaction);
        }

        public void UpdateTransactionConfirmations(uint blockHeight)
        {
            foreach (var transaction in this.transactions.Values)
            {
                var confirmations = blockHeight - transaction.Height + 1;

                transaction.Confirmations = confirmations;
            }
        }

        #endregion
    }
}
