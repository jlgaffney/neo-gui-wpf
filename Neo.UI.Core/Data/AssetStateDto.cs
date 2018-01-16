namespace Neo.UI.Core.Data
{
    public class AssetStateDto
    {
        public string Id { get; private set; }

        public string Owner { get; private set; }

        public string Admin { get; private set; }

        public string Total { get; private set; }

        public string Issued { get; private set; }

        public bool DistributionEnabled { get; private set; }

        public AssetStateDto(string id, string owner, string admin, string total, string issued, bool distributionEnabled)
        {
            this.Id = id;
            this.Owner = owner;
            this.Admin = admin;
            this.Total = total;
            this.Issued = issued;
            this.DistributionEnabled = distributionEnabled;
        }

        public string GetName()
        {
            return "Need to be implemented";
        }
    }
}
