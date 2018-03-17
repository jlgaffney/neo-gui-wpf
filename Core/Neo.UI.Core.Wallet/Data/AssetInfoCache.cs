using System;
using System.Collections.Generic;

namespace Neo.UI.Core.Wallet.Data
{
    internal class AssetInfoCache
    {
        private readonly IDictionary<UInt256, AssetInfo> assets;
        private readonly IDictionary<UInt160, NEP5TokenInfo> nep5Tokens;

        public AssetInfoCache()
        {
            this.assets = new Dictionary<UInt256, AssetInfo>();
            this.nep5Tokens = new Dictionary<UInt160, NEP5TokenInfo>();
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

        public IEnumerable<AssetInfo> GetAllAssetInfo()
        {
            return this.assets.Values;
        }

        public AssetInfo GetAssetInfo(UInt256 assetId)
        {
            if (assetId == null) return null;

            if (!this.assets.ContainsKey(assetId)) return null;

            return this.assets[assetId];
        }

        #endregion

        #region NEP-5 Tokens

        public void AddNEP5TokenInfo(NEP5TokenInfo token)
        {
            if (this.nep5Tokens.ContainsKey(token.ScriptHash))
            {
                throw new InvalidOperationException("Token has already been added!");
            }

            this.nep5Tokens.Add(token.ScriptHash, token);
        }

        public IEnumerable<NEP5TokenInfo> GetAllNEP5TokenInfo()
        {
            return this.nep5Tokens.Values;
        }

        public NEP5TokenInfo GetNEP5TokenInfo(UInt160 scriptHash)
        {
            if (scriptHash == null) return null;

            if (!this.nep5Tokens.ContainsKey(scriptHash)) return null;

            return this.nep5Tokens[scriptHash];
        }

        #endregion
    }
}
