using System;
using System.ComponentModel;
using System.Globalization;

namespace Neo.UI.Core.Converters
{
    public class UIntBaseConverter : TypeConverter
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
            if (value is string s) return context.PropertyDescriptor.PropertyType.GetMethod("Parse").Invoke(null, new[] { s });

            throw new NotSupportedException();
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string)) throw new NotSupportedException();

            var i = value as UIntBase;

            if (i == null) return null;

            return i.ToString();
        }
    }
}