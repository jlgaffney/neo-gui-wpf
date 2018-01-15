using Neo.UI.Core.Data;

namespace Neo.Gui.ViewModels.Tests.Builders
{
    public class AccountItemBuilder
    {
        private AccountType accountType = AccountType.Standard;
        private string labelInternal = "accountLabel";
        private UInt160 hashInternal;
        private Fixed8 neoBalance = Fixed8.Zero;

        public AccountItemBuilder WithLabel(string label)
        {
            this.labelInternal = label;
            return this;
        }

        public AccountItemBuilder WithHash(string hash)
        {
            this.hashInternal = UInt160.Parse(hash);
            return this;
        }
        
        public AccountItemBuilder StandardAccount()
        {
            this.accountType = AccountType.Standard;
            return this;
        }

        public AccountItemBuilder NonStandardAccount()
        {
            this.accountType = AccountType.NonStandard;
            return this;
        }

        public AccountItemBuilder WatchOnlyAccount()
        {
            this.accountType = AccountType.WatchOnly;
            return this;
        }

        public AccountItemBuilder AccountWithNeoBalance()
        {
            this.neoBalance = new Fixed8(1);
            return this;
        }

        public AccountItem Build()
        {
            var account = new AccountItem(this.labelInternal, this.hashInternal, this.accountType);

            if (this.neoBalance != Fixed8.Zero)
            {
                account.Neo = this.neoBalance;
            }

            return account;
        }
    }
}
