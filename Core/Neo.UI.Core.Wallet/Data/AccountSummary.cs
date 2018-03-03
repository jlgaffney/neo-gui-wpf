using Neo.UI.Core.Data.Enums;
using NeoWallet = Neo.Wallets.Wallet;

namespace Neo.UI.Core.Wallet.Data
{
    internal class AccountSummary
    {
        #region Public Properties 
        public string Label { get; }

        public UInt160 ScriptHash { get; }

        public string Address => NeoWallet.ToAddress(this.ScriptHash);

        public AccountType Type { get; }

        public AccountBalanceSummary BalanceSummary { get; }
        #endregion

        #region Constructor 
        public AccountSummary(string label, UInt160 scriptHash, AccountType accountType)
        {
            this.Label = label;
            this.ScriptHash = scriptHash;
            this.Type = accountType;

            this.BalanceSummary = new AccountBalanceSummary();
        }
        #endregion
    }
}
