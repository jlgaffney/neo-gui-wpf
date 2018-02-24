using System;
using System.ComponentModel;
using System.Globalization;

namespace Neo.UI.Core.Converters
{
    public class Fixed8Converter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;

            if (value != null) return Fixed8.Parse(str);

            throw new NotSupportedException();
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string)) throw new NotSupportedException();

            var f = (Fixed8)value;

            return f.ToString();
        }
    }
}