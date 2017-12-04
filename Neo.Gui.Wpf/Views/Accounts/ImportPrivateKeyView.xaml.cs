using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public partial class ImportPrivateKeyView  : IDialog<ImportPrivateKeyDialogResult>
    {
        public ImportPrivateKeyView()
        {
            InitializeComponent();
        }
    }
}