using System.Collections.Generic;

namespace Neo.UI.Accounts
{
    public partial class ImportPrivateKeyView
    {
        private readonly ImportPrivateKeyViewModel viewModel;

        public ImportPrivateKeyView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as ImportPrivateKeyViewModel;
        }

        public IEnumerable<string> WifStrings => this.viewModel?.WifStrings;
    }
}