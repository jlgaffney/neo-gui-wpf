using System.ComponentModel;
using Neo.Gui.Base.Converters;
using Neo.Gui.Base.Localization;

namespace Neo.Gui.Base.Theming
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Style
    {
        //[LocalizedDescription(nameof(EnumStrings.Light), typeof(EnumStrings))]
        Light,

        //[LocalizedDescription(nameof(EnumStrings.Dark), typeof(EnumStrings))]
        Dark
    }
}