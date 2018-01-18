using System.ComponentModel;

namespace Neo.UI.Core.Data
{
    public enum AssetTypeDto
    {
        [Description("Choose asset type")]
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
}
