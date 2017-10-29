using Neo.Wallets;

namespace Neo.UI
{
    public class AccountItem
    {
        public string Address { get; set; }

        public AccountType Type { get; set; }

        public Fixed8 Neo { get; set; }

        public Fixed8 Gas { get; set; }

        public UInt160 ScriptHash { get; set; }

        public VerificationContract Contract {get;set;}
    }
}