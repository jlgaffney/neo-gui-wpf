using System.Collections.Generic;

namespace Neo.UI.Messages
{
    public class AccountItemsChangedMessage
    {
        public IEnumerable<AccountItem> Accounts { get; private set; }

        public AccountItemsChangedMessage(IEnumerable<AccountItem> accounts)
        {
            this.Accounts = accounts;
        }
    }
}
