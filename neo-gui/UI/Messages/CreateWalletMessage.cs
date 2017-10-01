namespace Neo.UI.Messages
{
    public class CreateWalletMessage
    {
        public CreateWalletMessage(string walletPath, string password)
        {
            this.WalletPath = walletPath;
            this.Password = password;
        }

        public string WalletPath { get; set; }

        public string Password { get; set; }
    }
}