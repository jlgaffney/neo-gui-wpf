using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Neo.Gui.Cross.Converters
{
    public sealed class IsNotNullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && bool.Parse((string) parameter))
            {
                return value is null;
            }

            return !(value is null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}