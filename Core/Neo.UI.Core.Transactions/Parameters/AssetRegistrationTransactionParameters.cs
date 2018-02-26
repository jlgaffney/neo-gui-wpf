using System.Globalization;

using Neo.UI.Core.Data;

namespace Neo.UI.Core.Transactions.Parameters
{
    public class AssetRegistrationTransactionParameters : TransactionParameters
    {
        public AssetTypeDto AssetType { get; }

        public string Name { get; }

        public string FormattedName => !string.IsNullOrWhiteSpace(this.Name)
            ? $"[{{\"lang\":\"{CultureInfo.CurrentCulture.Name}\",\"name\":\"{this.Name}\"}}]"
            : string.Empty;

        public bool IsTotalTokenAmountLimited { get; }

        public string TotalTokenAmount { get; }

        public int Precision { get; }

        public string OwnerKey { get; }

        public string AdminAddress { get; }

        public string IssuerAddress { get; }

        public AssetRegistrationTransactionParameters(
            AssetTypeDto assetTypeDto, 
            string name, 
            bool isTotalTokenAmountLimited,
            string totalTokenAmount,
            int precision, 
            string ownerKey,
            string adminAddress, 
            string issuerAddress)
        {
            this.AssetType = assetTypeDto;
            this.Name = name;
            this.IsTotalTokenAmountLimited = isTotalTokenAmountLimited;
            this.TotalTokenAmount = totalTokenAmount;
            this.Precision = precision;
            this.OwnerKey = ownerKey;
            this.AdminAddress = adminAddress;
            this.IssuerAddress = issuerAddress;
        }
    }
}
