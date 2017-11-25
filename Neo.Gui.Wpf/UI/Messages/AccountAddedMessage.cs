namespace Neo.UI.Messages
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