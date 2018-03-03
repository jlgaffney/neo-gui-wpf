using System.Collections.Generic;
using System.Linq;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Wallet.Data
{
    internal class WalletInfo
    {
        private readonly IDictionary<UInt160, AccountSummary> accounts;
        private readonly IList<AssetSummary> assets;
        private readonly IList<TransactionItem> transactions;

        public WalletInfo()
        {
            this.accounts = new Dictionary<UInt160, AccountSummary>();
            this.assets = new List<AssetSummary>();
            this.transactions = new List<TransactionItem>();
        }

        public IEnumerable<AccountSummary> GetAccounts()
        {
            return this.accounts.Values;
        }

        public bool ContainsAccount(UInt160 scriptHash)
        {
            return this.accounts.ContainsKey(scriptHash);
        }

        public AccountSummary GetAccount(UInt160 scriptHash)
        {
            return this.ContainsAccount(scriptHash) ? this.accounts[scriptHash] : null;
        }

        public void AddAccount(AccountSummary account)
        {
            this.accounts.Add(UInt160.Parse(account.ScriptHash), account);
        }

        public void RemoveAccount(UInt160 scriptHash)
        {
            this.accounts.Remove(scriptHash);
        }

        public IEnumerable<FirstClassAssetSummary> GetFirstClassAssets()
        {
            return this.assets.Where(item => item is FirstClassAssetSummary).Cast<FirstClassAssetSummary>();
        }

        public IEnumerable<NEP5AssetSummary> GetNEP5Assets()
        {
            return this.assets.Where(item => item is NEP5AssetSummary).Cast<NEP5AssetSummary>();
        }

        public FirstClassAssetSummary GetFirstClassAsset(UInt256 assetId)
        {
            if (assetId == null) return null;

            var assetIdStr = assetId.ToString();

            return this.assets.FirstOrDefault(asset => asset is FirstClassAssetSummary &&
                assetIdStr.Equals(((FirstClassAssetSummary) asset).AssetId)) as FirstClassAssetSummary;
        }

        public NEP5AssetSummary GetNEP5Asset(UInt160 scriptHash)
        {
            if (scriptHash == null) return null;

            var scriptHashStr = scriptHash.ToString();

            return this.assets.FirstOrDefault(asset => asset is NEP5AssetSummary &&
                scriptHashStr.Equals(((NEP5AssetSummary) asset).ScriptHash)) as NEP5AssetSummary;
        }

        public void AddAsset(AssetSummary asset)
        {
            this.assets.Add(asset);
        }

        public void RemoveAsset(AssetSummary asset)
        {
            this.assets.Remove(asset);
        }

        public void AddTransaction(TransactionItem transaction)
        {
            // Add transaction to beginning of list
            this.transactions.Insert(0, transaction);
        }

        public void UpdateTransactionConfirmations(uint blockHeight)
        {
            foreach (var transactionItem in this.transactions)
            {
                var confirmations = blockHeight - transactionItem.Height + 1;

                transactionItem.Confirmations = confirmations;
            }
        }

        public IEnumerable<TransactionItem> GetTransactions()
        {
            return this.transactions;
        }
    }
}
