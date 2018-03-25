namespace Neo.UI.Core.Wallet.Initialization
{
    public class LightWalletInitializationParameters : BaseWalletInitializationParameters
    {
        public LightWalletInitializationParameters(string[] rpcSeedList, string certificateCachePath) : base(certificateCachePath)
        {
            this.RpcSeedList = rpcSeedList;
        }

        internal string[] RpcSeedList { get; }
    }
}
