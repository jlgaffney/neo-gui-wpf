namespace Neo.Gui.Cross.Models
{
    public class AssetSummary
    {
        public string Id { get; set; }

        public string Name { get; set; }
        
        public AssetType Type { get; set; }

        public string IssuerAddress { get; set; }

        public string Balance { get; set; }
    }
}
