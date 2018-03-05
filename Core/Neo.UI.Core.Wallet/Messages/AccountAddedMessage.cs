using Neo.UI.Core.Data.Enums;

namespace Neo.UI.Core.Wallet.Messages
{
    public class AccountAddedMessage
    {
        public string AccountLabel { get; }

        public string AccountAddress { get; }

        public string AccountScriptHash { get; }

        public AccountType AccountType { get; }

        public AccountAddedMessage(string label, string address, string scriptHash, AccountType type)
        {
            this.AccountLabel = label;
            this.AccountAddress = address;
            this.AccountScriptHash = scriptHash;
            this.AccountType = type;
        }
    }
}