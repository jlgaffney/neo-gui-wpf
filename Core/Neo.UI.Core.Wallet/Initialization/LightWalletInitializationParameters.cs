namespace Neo.UI.Core.Wallet.Initialization
{
    public class LightWalletInitializationParameters : IWalletInitializationParameters
    {
        public LightWalletInitializationParameters(string[] rpcSeedList)
        {
            this.RpcSeedList = rpcSeedList;
        }

        internal string[] RpcSeedList { get; }
    }
}
