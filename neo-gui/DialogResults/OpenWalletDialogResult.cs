namespace Neo.DialogResults
{
    public class OpenWalletDialogResult
    {
        public string WalletPath { get; private set; }

        public string Password { get; private set; }

        public bool OpenInRepairMode { get; private set; }

        public OpenWalletDialogResult(string walletPath, string password, bool openInRepairMode)
        {
            this.WalletPath = walletPath;
            this.Password = password;
            this.OpenInRepairMode = OpenInRepairMode;
        }
    }
}
