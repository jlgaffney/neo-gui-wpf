namespace Neo.UI.Messages
{
    public class NewVersionAvailableMessage
    {
        public string NewVersionLabel { get; private set; }

        public NewVersionAvailableMessage(string newVersionLabel)
        {
            this.NewVersionLabel = newVersionLabel;
        }
    }
}
