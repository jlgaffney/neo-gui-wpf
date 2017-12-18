using Neo.Wallets;

namespace Neo.Gui.Base.Dialogs.LoadParameters.Accounts
{
    public class ViewPrivateKeyLoadParameters
    {
        public WalletAccount Account { get; }

        public ViewPrivateKeyLoadParameters(WalletAccount account)
        {
            this.Account = account;
        }
    }
}
