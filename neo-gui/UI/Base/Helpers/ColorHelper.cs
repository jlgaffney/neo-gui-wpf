using System;
using System.Windows.Media;

namespace Neo.UI.Base.Helpers
{
    public static class ColorHelper
    {
        /// <summary>
        /// Convert a hex color string to a <c>System.Windows.Media.Color</c> instance.
        /// </summary>
        /// <param name="hex">Hexadecimal color string</param>
        /// <param name="defaultColor">Default color value to return if conversion fails</param>
        public static Color HexToColor(string hex, Color? defaultColor = null)
        {
            try
            {
                return (Color) ColorConverter.ConvertFromString(hex);
            }
            catch
            {
                // Return color with default values if available
                return defaultColor ?? new Color();
            }
        }

        public static string ColorToHex(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        /// <summary>
        /// Sets the color's transparency to the specified fraction.
        /// </summary>
        /// <param name="color">Color to apply transparency to</param>
        /// <param name="transparencyFraction">Value from <c>0.0</c> to <c>1.0</c> inclusively</param>
        /// <returns>Color with transparency set to the specified fraction</returns>
        public static Color SetTransparencyFraction(Color color, double transparencyFraction)
        {
            // Set value to be within valid range if necessary
            if (transparencyFraction < 0.0)
            {
                transparencyFraction = 0.0;
            }
            else if (transparencyFraction > 1.0)
            {
                transparencyFraction = 1.0;
            }

            var transparency = (byte) Math.Round(byte.MaxValue * transparencyFraction);

            color.A = transparency;

            return color;
        }
    }
}