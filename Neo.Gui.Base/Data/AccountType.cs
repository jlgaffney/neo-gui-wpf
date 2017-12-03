using System.ComponentModel;
using Neo.Gui.Base.Converters;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Localization;

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