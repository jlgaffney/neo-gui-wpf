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

            this.BaseTransaction = baseTransaction;
        }

        public InvocationTransaction BaseTransaction { get; }

        public void SetSelectedTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= this.TabControl.Items.Count) throw new IndexOutOfRangeException();

            this.TabControl.SelectedIndex = tabIndex;
        }
    }
}