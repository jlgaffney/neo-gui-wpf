using System.ComponentModel;
using Neo.Gui.Base.Converters;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Localization;

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