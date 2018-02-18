namespace Neo.UniversalWallet.Messages
{
    public class NavigationMessage
    {
        public string DestinationPage { get; private set; }

        public object[] Parameters { get; private set; }

        public NavigationMessage(string destinationPage, params object[] parameters)
        {
            this.DestinationPage = destinationPage;
            this.Parameters = parameters;
        }
    }
}
