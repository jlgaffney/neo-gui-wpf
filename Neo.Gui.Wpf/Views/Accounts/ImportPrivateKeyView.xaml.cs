using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Accounts;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public partial class ImportPrivateKeyView  : IDialog<ImportPrivateKeyLoadParameters>
    {
        public ImportPrivateKeyView()
        {
            InitializeComponent();
        }
    }
}