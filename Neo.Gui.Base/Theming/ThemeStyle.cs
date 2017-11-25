using System.ComponentModel;
using Neo.UI.Base.Converters;
using Neo.UI.Base.Localization;

namespace Neo.Gui.Base.Theming
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ThemeStyle
    {
        //[LocalizedDescription(nameof(EnumStrings.Light), typeof(EnumStrings))]
        Light,

        //[LocalizedDescription(nameof(EnumStrings.Dark), typeof(EnumStrings))]
        Dark
    }
}