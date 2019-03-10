using Neo.Gui.Cross.Models;
using Neo.SmartContract;
using Neo.Wallets;

namespace Neo.Gui.Cross.Extensions
{
    public static class WalletAccountExtensions
    {
        public static AccountType GetAccountType(this WalletAccount account)
        {
            if (account.WatchOnly)
            {
                return AccountType.WatchOnly;
            }

            return account.Contract.Script.IsSignatureContract()
                ? AccountType.Standard
                : AccountType.NonStandard;
        }
    }
}
