using System.ComponentModel;
using Neo.Gui.Cross.Localization;
using Neo.Gui.Cross.Resources;

namespace Neo.Gui.Cross.Models
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
