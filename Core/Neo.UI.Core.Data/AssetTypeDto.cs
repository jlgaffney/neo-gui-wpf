using System.Collections.Generic;
using Neo.Core;

namespace Neo.UI.Core.Data
{
    public enum AssetTypeDto
    {
        None,
        GoverningToken,
        UtilityToken,
        Currency,
        CreditFlag,
        Token,
        DutyFlag,
        Share,
        Invoice
    }

    public static class AssetTypeDtoExtensions
    {
        public static AssetType ToNeoAssetType(this AssetTypeDto assetTypeDto)
        {
            var mappingDictionary = new Dictionary<AssetTypeDto, AssetType>
            {
                { AssetTypeDto.CreditFlag, AssetType.CreditFlag },
                { AssetTypeDto.Currency, AssetType.Currency },
                { AssetTypeDto.DutyFlag, AssetType.DutyFlag },
                { AssetTypeDto.GoverningToken, AssetType.GoverningToken },
                { AssetTypeDto.Invoice, AssetType.Invoice },
                { AssetTypeDto.Share, AssetType.Share },
                { AssetTypeDto.Token, AssetType.Token },
                { AssetTypeDto.UtilityToken, AssetType.UtilityToken }
            };

            return mappingDictionary[assetTypeDto];
        }
    }
}
