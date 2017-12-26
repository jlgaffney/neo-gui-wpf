using System.ComponentModel;
using Neo.Gui.Base.Converters;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.Globalization.Localization;

namespace Neo.Gui.Base.Data
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AccountType
    {
        [LocalizedDescription(nameof(Strings.Standard), typeof(Strings))]
        Standard,

        [LocalizedDescription(nameof(Strings.NonStandard), typeof(Strings))]
        NonStandard,

        [LocalizedDescription(nameof(Strings.WatchOnly), typeof(Strings))]
        WatchOnly
    }
}