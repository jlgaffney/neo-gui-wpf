using Neo.Wallets;

namespace Neo.UI.Models
{
    public class AccountItem
    {
        public string Address { get; set; }

        public Fixed8 NEO { get; set; }

        public Fixed8 GAS { get; set; }

        public UInt160 ScriptHash { get; set; }

        public VerificationContract Contract {get;set;}
    }
}