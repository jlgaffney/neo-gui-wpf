using System.ComponentModel;
using Neo.Gui.Globalization.Localization;
using Neo.Gui.Globalization.Resources;
using Neo.UI.Core.Converters;

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