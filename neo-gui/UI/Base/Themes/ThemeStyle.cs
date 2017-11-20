using System.ComponentModel;
using Neo.UI.Base.Converters;
using Neo.UI.Base.Localization;
using Neo.UI.Base.Resources;

namespace Neo.UI.Base.Themes
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ThemeStyle
    {
        [LocalizedDescription(nameof(EnumStrings.Light), typeof(EnumStrings))]
        Light,

        [LocalizedDescription(nameof(EnumStrings.Dark), typeof(EnumStrings))]
        Dark
    }
}