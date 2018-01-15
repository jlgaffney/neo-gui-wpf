using Neo.UI.Core.Data;

namespace Neo.UI.Core.Messages
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