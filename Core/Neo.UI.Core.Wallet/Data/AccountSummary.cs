using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Wallet.Data;
using NeoWallet = Neo.Wallets.Wallet;

namespace Neo.UI.Core.Data
{
    public class AccountSummary : BindableClass
    {
        #region Public Properties 
        public string Label { get; }

        public string ScriptHash { get; }

        public string Address => NeoWallet.ToAddress(UInt160.Parse(this.ScriptHash));

        public AccountType Type { get; }

        public AccountBalanceSummary BalanceSummary { get; }
        #endregion

        #region Constructor 
        public AccountSummary(string label, string scriptHash, AccountType accountType)
        {
            this.Label = label;
            this.ScriptHash = scriptHash;
            this.Type = accountType;

            this.BalanceSummary = new AccountBalanceSummary();
        }
        #endregion
    }
}
