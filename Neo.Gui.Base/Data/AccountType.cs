using System.ComponentModel;
//using Neo.Gui.Wpf.UI.Base.Resources;
using Neo.UI.Base.Converters;
using Neo.UI.Base.Localization;

namespace Neo.UI
{
    //[TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AccountType
    {
        //[LocalizedDescription(nameof(EnumStrings.Standard), typeof(EnumStrings))]
        Standard,

        //[LocalizedDescription(nameof(EnumStrings.NonStandard), typeof(EnumStrings))]
        NonStandard,

        //[LocalizedDescription(nameof(EnumStrings.WatchOnly), typeof(EnumStrings))]
        WatchOnly
    }
}