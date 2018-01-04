using System.ComponentModel;

using Neo.Gui.Globalization.Resources;
using Neo.Gui.Globalization.Localization;

using Neo.Gui.Base.Converters;

namespace Neo.Gui.Base.Theming
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