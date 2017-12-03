using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace Neo.Gui.Wpf.Extensions
{
    public static class ColorExtensions
    {
        public static MediaColor ToMediaColor(this DrawingColor color)
        {
            return MediaColor.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
