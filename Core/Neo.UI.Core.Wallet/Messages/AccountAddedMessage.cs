using Neo.UI.Core.Data;

namespace Neo.UI.Core.Wallet.Messages
{
    public class AccountAddedMessage
    {
        public AccountItem Account { get; }

        public AccountAddedMessage(AccountItem account)
        {
            this.Account = account;
        }
    }
}