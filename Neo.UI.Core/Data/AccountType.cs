using System.ComponentModel;
using Neo.Gui.Globalization.Localization;
using Neo.Gui.Globalization.Resources;
using Neo.UI.Core.Converters;

namespace Neo.UI.Core.Data
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