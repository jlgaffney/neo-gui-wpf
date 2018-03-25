using System;
using System.Collections.Generic;

namespace Neo.UI.Core.Wallet.Data
{
    internal class AssetInfoCache
    {
        private readonly IDictionary<UInt256, AssetInfo> assets;

        public AssetInfoCache()
        {
            this.assets = new Dictionary<UInt256, AssetInfo>();
        }

        #region Assets

        public void AddAssetInfo(AssetInfo asset)
        {
            if (this.assets.ContainsKey(asset.AssetId))
            {
                throw new InvalidOperationException("Asset has already been added!");
            }

            this.assets.Add(asset.AssetId, asset);
        }

        public AssetInfo GetAssetInfo(UInt256 assetId)
        {
            if (assetId == null) return null;

            if (!this.assets.ContainsKey(assetId)) return null;

            return this.assets[assetId];
        }

        #endregion
    }
}
