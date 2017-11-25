using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Neo.Properties
{
    internal sealed partial class Settings
    {
        public bool RemoteNodeMode { get; }
        public string DataDirectoryPath { get; }
        public string CertCachePath { get; }
        public ushort NodePort { get; }
        public ushort WsPort { get; }
        public BrowserSettings Urls { get; }
        public ContractSettings Contracts { get; }

        public Settings()
        {
            if (NeedUpgrade)
            {
                Upgrade();
                NeedUpgrade = false;
                Save();
            }
            var section = new ConfigurationBuilder().AddJsonFile("config.json").Build().GetSection("ApplicationConfiguration");

            var remoteNodeModeSection = section.GetSection("RemoteNodeMode");
            this.RemoteNodeMode = !string.IsNullOrEmpty(remoteNodeModeSection?.Value) && bool.Parse(remoteNodeModeSection.Value);

            this.DataDirectoryPath = section.GetSection("DataDirectoryPath").Value;

            this.CertCachePath = section.GetSection("CertCachePath").Value;

            this.NodePort = ushort.Parse(section.GetSection("NodePort").Value);

            this.WsPort = ushort.Parse(section.GetSection("WsPort").Value);

            this.Urls = new BrowserSettings(section.GetSection("Urls"));

            this.Contracts = new ContractSettings(section.GetSection("Contracts"));
        }
    }

    internal class BrowserSettings
    {
        public string AddressUrl { get; }
        public string AssetUrl { get; }
        public string TransactionUrl { get; }

        public BrowserSettings(IConfigurationSection section)
        {
            this.AddressUrl = section.GetSection("AddressUrl").Value;
            this.AssetUrl = section.GetSection("AssetUrl").Value;
            this.TransactionUrl = section.GetSection("TransactionUrl").Value;
        }
    }

    internal class ContractSettings
    {
        public UInt160[] NEP5 { get; }

        public ContractSettings(IConfigurationSection section)
        {
            this.NEP5 = section.GetSection("NEP5").GetChildren().Select(p => UInt160.Parse(p.Value)).ToArray();
        }
    }
}
