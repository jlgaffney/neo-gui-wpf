using System;
using Neo.Core;

namespace Neo.UI.Contracts
{
    /// <summary>
    /// Interaction logic for InvokeContractView.xaml
    /// </summary>
    public partial class InvokeContractView
    {
        public InvokeContractView(InvocationTransaction baseTransaction)
        {
            InitializeComponent();

            var viewModel = this.DataContext as InvokeContractViewModel;

            if (viewModel == null) return;

            viewModel.SetBaseTransaction(baseTransaction);

            this.SetSelectedTab(1);
        }

        private void SetSelectedTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= this.TabControl.Items.Count) throw new IndexOutOfRangeException();

            this.TabControl.SelectedIndex = tabIndex;
        }
    }
}