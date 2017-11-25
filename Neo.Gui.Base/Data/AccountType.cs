using System.ComponentModel;
using Neo.Gui.Base.Converters;
using Neo.Gui.Base.Localization;
//using Neo.Gui.Wpf.UI.Base.Resources;

namespace Neo.Gui.Base.Data
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
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