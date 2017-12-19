using Neo.Gui.Base.Data;

namespace Neo.Gui.Base.Messages
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