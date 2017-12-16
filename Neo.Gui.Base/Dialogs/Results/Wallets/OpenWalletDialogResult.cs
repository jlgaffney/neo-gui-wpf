namespace Neo.Gui.Base.Dialogs.Results
{
    public class OpenWalletDialogResult
    {
        public string WalletPath { get; }

        public string Password { get; }

        public OpenWalletDialogResult(string walletPath, string password)
        {
            this.WalletPath = walletPath;
            this.Password = password;
        }
    }
}
