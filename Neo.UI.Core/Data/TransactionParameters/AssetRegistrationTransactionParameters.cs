using System.Globalization;

namespace Neo.UI.Core.Data.TransactionParameters
{
    public class AssetRegistrationTransactionParameters
    {
        public AssetTypeDto AssetType { get; private set; }

        public string Name { get; private set; }

        public string FormatedName { get; private set; }

        public bool IsTotalTokenAmountLimited { get; private set; }

        public string TotalTokenAmount { get; private set; }

        public int Precision { get; private set; }

        public string OwnerKey { get; private set; }

        public string AdminAddress { get; private set; }

        public string IssuerAddress { get; private set; }

        public AssetRegistrationTransactionParameters(
            AssetTypeDto assetTypeDto, 
            string name, 
            bool isTotalTokenAmountLimited,
            string totalTokenAmount,
            int precision, 
            string ownerKey,
            string adminAddress, 
            string IssuerAddress)
        {
            this.AssetType = assetTypeDto;
            this.Name = name;
            this.IsTotalTokenAmountLimited = isTotalTokenAmountLimited;
            this.TotalTokenAmount = totalTokenAmount;
            this.Precision = precision;
            this.OwnerKey = ownerKey;
            this.AdminAddress = adminAddress;
            this.IssuerAddress = IssuerAddress;

            this.FormatedName = !string.IsNullOrWhiteSpace(this.Name)
                    ? $"[{{\"lang\":\"{CultureInfo.CurrentCulture.Name}\",\"name\":\"{this.Name}\"}}]"
                    : string.Empty;
        }
    }
}
