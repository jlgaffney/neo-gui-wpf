namespace Neo.Gui.Base.Dialogs.Results.Wallets
{
    public class TradeVerificationDialogResult
    {
        public bool TradeAccepted { get; private set; }

        public TradeVerificationDialogResult(bool tradeAccepted)
        {
            this.TradeAccepted = tradeAccepted;
        }
    }
}
