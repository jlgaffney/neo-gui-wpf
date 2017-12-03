using System.Collections.Generic;
using Neo.Core;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public partial class TradeVerificationView : IDialog<TradeVerificationDialogResult>
    {
        public TradeVerificationView(IEnumerable<TransactionOutput> outputs)
        {
            InitializeComponent();
        }
    }
}