using System.Collections.Generic;
using Neo.UI.ViewModels.Accounts;

namespace Neo.UI.Views.Accounts
{
    public partial class ImportPrivateKeyView
    {
        private ImportPrivateKeyViewModel viewModel;

        public ImportPrivateKeyView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as ImportPrivateKeyViewModel;
        }

        public IEnumerable<string> WifStrings => this.viewModel?.WifStrings;
    }
}