using System.Collections.Generic;

using Neo.Core;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Extensions
{
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
