using System;
using Neo.Core;

namespace Neo.UI.Contracts
{
    /// <summary>
    /// Interaction logic for InvokeContractView.xaml
    /// </summary>
    public partial class InvokeContractView
    {
        private readonly InvokeContractViewModel viewModel;

        public InvokeContractView(InvocationTransaction baseTransaction = null)
        {
            InitializeComponent();

            this.viewModel = this.DataContext as InvokeContractViewModel;

            this.BaseTransaction = baseTransaction;
        }

        public InvocationTransaction BaseTransaction { get; }

        public InvocationTransaction GetTransaction()
        {
            return this.viewModel?.GetTransaction();
        }

        public void SetSelectedTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= this.TabControl.Items.Count) throw new IndexOutOfRangeException();

            this.TabControl.SelectedIndex = tabIndex;
        }
    }
}