using Neo.Implementations.Wallets.EntityFramework;

namespace Neo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static UserWallet CurrentWallet;

        internal App()
        {
            this.InitializeComponent();
        }
    }
}