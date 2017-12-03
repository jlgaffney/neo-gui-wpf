using System.Collections.Generic;
using Neo.Core;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public partial class TradeVerificationView
    {
        private readonly TradeVerificationViewModel viewModel;

        public TradeVerificationView(IEnumerable<TransactionOutput> outputs)
        {
            InitializeComponent();

            this.viewModel = this.DataContext as TradeVerificationViewModel;

            this.viewModel?.SetOutputs(outputs);
        }
        
        public bool TradeAccepted => viewModel?.TradeAccepted ?? false;
    }
}