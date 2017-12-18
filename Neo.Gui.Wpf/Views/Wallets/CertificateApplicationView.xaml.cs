using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Wallets;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public partial class CertificateApplicationView : IDialog<CertificateApplicationDialogResult>
    {
        public CertificateApplicationView()
        {
            InitializeComponent();
        }
    }
}
