using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Wallets;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public partial class CertificateApplicationView : IDialog<CertificateApplicationLoadParameters>
    {
        public CertificateApplicationView()
        {
            InitializeComponent();
        }
    }
}
