using Neo.UI.Core.Data;

namespace Neo.Gui.Dialogs.LoadParameters.Contracts
{
    public class AssetRegistrationParameters
    {
        public AssetTypeDto AssetType { get; private set; }

        public string FormatedName { get; private set; }

        public decimal TotalTokenAmount { get; private set; }

        public int Precision { get; private set; }

        public string OwnerKey { get; private set; }

        public string AdminAddress { get; private set; }

        public string IssuerAddress { get; private set; }

        public AssetRegistrationParameters(
            AssetTypeDto assetTypeDto, 
            string formatedName, 
            decimal totalTokenAmount,
            int precision, 
            string ownerKey,
            string adminAddress, 
            string IssuerAddress)
        {
            this.AssetType = assetTypeDto;
            this.FormatedName = formatedName;
            this.Precision = precision;
            this.OwnerKey = ownerKey;
            this.AdminAddress = adminAddress;
            this.IssuerAddress = IssuerAddress;
        }
    }
}
