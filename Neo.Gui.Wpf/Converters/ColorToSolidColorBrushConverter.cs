using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using Neo.Gui.Wpf.Extensions;

using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace Neo.Gui.Wpf.Converters
{
    public class ColorToSolidColorBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value) return null;

            if (value is DrawingColor drawingColor)
            {
                return new SolidColorBrush(drawingColor.ToMediaColor());
            }

            if (value is MediaColor mediaColor)
            {
                return new SolidColorBrush(mediaColor);
            }

            var type = value.GetType();

            throw new InvalidOperationException("Unsupported type [" + type.Name + "]");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
