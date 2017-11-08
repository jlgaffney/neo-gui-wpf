namespace Neo.UI.Messages
{
    public class UpdateAssetsBalanceMessage
    {
        public bool BalanceChanged { get; private set; }

        public UpdateAssetsBalanceMessage(bool balanceChanged)
        {
            this.BalanceChanged = balanceChanged;
        }
    }
}
