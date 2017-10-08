using System.Collections.Generic;
using Neo.Core;

namespace Neo.UI.Wallets
{
    public partial class TradeVerificationView
    {
        public TradeVerificationView(IEnumerable<TransactionOutput> outputs)
        {
            InitializeComponent();

            var viewModel = this.DataContext as TradeVerificationViewModel;

            viewModel?.SetOutputs(outputs);
        }
    }
}