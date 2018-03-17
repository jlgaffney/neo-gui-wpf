namespace Neo.UI.Core.Data
{
    public class AssetStateDto
    {
        public string Id { get; }

        public string Owner { get; }

        public string Admin { get; }

        public string Amount { get; }

        public string Available { get; }

        public int Precision { get; }

        public string Name { get; }

        public AssetStateDto(string id, string owner, string admin, string amount, string available, int precision, string name)
        {
            this.Id = id;
            this.Owner = owner;
            this.Admin = admin;
            this.Amount = amount;
            this.Available = available;
            this.Precision = precision;
            this.Name = name;
        }
    }
}
