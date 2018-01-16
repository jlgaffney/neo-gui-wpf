namespace Neo.Gui.Dialogs.LoadParameters.Assets
{
    public class AssetDistributionLoadParameters
    {
        public string AssetStateId { get; private set; }

        public AssetDistributionLoadParameters(string assetStateId)
        {
            this.AssetStateId = assetStateId;
        }
    }
}
