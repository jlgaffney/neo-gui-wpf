using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Wallets;

namespace Neo.Gui.Wpf.Views.Wallets
{
    /// <summary>
    /// Interaction logic for ClaimView.xaml
    /// </summary>
    public partial class ClaimView : IDialog<ClaimLoadParameters>
    {
        public ClaimView()
        {
            InitializeComponent();
        }
    }
}