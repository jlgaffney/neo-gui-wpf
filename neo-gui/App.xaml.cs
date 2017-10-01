using System.Xml.Linq;

using Neo.Implementations.Wallets.EntityFramework;
using Neo.UI.Views;

namespace Neo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static UserWallet CurrentWallet;

        internal App(XDocument xdoc) : base()
        {
            this.MainWindow = new MainView(xdoc);

            this.MainWindow.Show();
        }
    }
}