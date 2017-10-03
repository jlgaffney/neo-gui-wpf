using System;
using Neo.Core;
using Neo.UI.ViewModels.Contracts;

namespace Neo.UI.Views.Contracts
{
    /// <summary>
    /// Interaction logic for DeveloperToolsView.xaml
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