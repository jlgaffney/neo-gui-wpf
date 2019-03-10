namespace Neo.Gui.Cross
{
    public interface ISettings
    {
        string LastWalletPath { get; set; }

        bool InstallCertificate { get; }

        PathsSettings Paths { get; }

        P2PSettings P2P { get; }

        BrowserSettings Urls { get; }

        ContractSettings Contracts { get; }
    }
}