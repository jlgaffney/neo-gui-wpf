using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Wallets;

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