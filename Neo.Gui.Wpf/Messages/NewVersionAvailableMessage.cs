namespace Neo.Gui.Wpf.Messages
{
    public class NewVersionAvailableMessage
    {
        public NewVersionAvailableMessage(string newVersionLabel)
        {
            this.NewVersionLabel = newVersionLabel;
        }

        public string NewVersionLabel { get; }
    }
}
