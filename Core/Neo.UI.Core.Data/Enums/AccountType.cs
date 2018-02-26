using System.ComponentModel;
using Neo.UI.Core.Globalization;
using Neo.UI.Core.Globalization.Resources;

namespace Neo.UI.Core.Data.Enums
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