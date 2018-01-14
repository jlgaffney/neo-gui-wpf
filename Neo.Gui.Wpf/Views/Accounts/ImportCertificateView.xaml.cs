using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Accounts;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public partial class ImportCertificateView : IDialog<ImportCertificateLoadParameters>
    {
        public ImportCertificateView()
        {
            InitializeComponent();
        }
    }
}