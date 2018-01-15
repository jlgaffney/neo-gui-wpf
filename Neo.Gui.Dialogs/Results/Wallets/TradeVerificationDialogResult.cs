namespace Neo.Gui.Dialogs.Results.Wallets
{
    public class TradeVerificationDialogResult
    {
        public bool TradeAccepted { get; }

        public TradeVerificationDialogResult(bool tradeAccepted)
        {
            this.TradeAccepted = tradeAccepted;
        }
    }
}
