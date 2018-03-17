using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.UI.Core.Wallet.Messages
{
    public class AccountBalanceRefreshedMessage
    {
        public string ScriptHash { get; }

        public int Neo { get; }

        public decimal Gas { get; }

        public AccountBalanceRefreshedMessage(string scriptHash, int neo, decimal gas)
        {
            this.ScriptHash = scriptHash;
            this.Neo = neo;
            this.Gas = gas;
        }
    }
}
