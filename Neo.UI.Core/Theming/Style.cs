using System.ComponentModel;
using Neo.UI.Core.Globalization;
using Neo.UI.Core.Globalization.Resources;

namespace Neo.UI.Core.Theming
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Style
    {
        [LocalizedDescription(nameof(Strings.Light), typeof(Strings))]
        Light,

        [LocalizedDescription(nameof(Strings.Dark), typeof(Strings))]
        Dark
    }
}