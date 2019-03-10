using System.ComponentModel;
using Neo.Gui.Cross.Localization;

namespace Neo.Gui.Cross.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AssetType
    {
        //[LocalizedDescription(nameof(Strings.SystemAsset), typeof(Strings))]
        SystemAsset,

        //[LocalizedDescription(nameof(Strings.GlobalAsset), typeof(Strings))]
        GlobalAsset,

        //[LocalizedDescription(nameof(Strings.NEP5Token), typeof(Strings))]
        NEP5Token
    }
}
