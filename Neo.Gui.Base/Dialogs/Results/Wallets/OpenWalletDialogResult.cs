namespace Neo.Gui.Base.Dialogs.Results
{
    public class OpenWalletDialogResult
    {
        public string WalletPath { get; }

        public string Password { get; }

        public bool OpenInRepairMode { get; }

        public OpenWalletDialogResult(string walletPath, string password, bool openInRepairMode)
        {
            this.WalletPath = walletPath;
            this.Password = password;
            this.OpenInRepairMode = openInRepairMode;
        }
    }
}
