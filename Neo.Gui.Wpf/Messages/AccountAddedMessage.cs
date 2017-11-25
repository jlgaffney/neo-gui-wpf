using Neo.Gui.Base.Data;

namespace Neo.Gui.Wpf.Messages
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