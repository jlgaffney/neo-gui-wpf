namespace Neo.Gui.Cross
{
    public interface ISettings
    {
        string LastWalletPath { get; set; }

        PathsSettings Paths { get; }

        P2PSettings P2P { get; }

        BrowserSettings Urls { get; }

        ContractSettings Contracts { get; }


        void Save();
    }
}