using System;
using System.Drawing;

namespace Neo.UI.Core.Extensions
{
    public static class ColorExtensions
    {
        public static string ToHex(this Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        /// <summary>
        /// Convert a hex color string to a <c>System.Windows.Media.Color</c> instance.
        /// </summary>
        /// <param name="hex">Hexadecimal color string</param>
        /// <param name="defaultColor">Default color value to return if conversion fails</param>
        public static Color HexToColor(this string hex, Color? defaultColor = null)
        {
            if (!defaultColor.HasValue)
            {
                defaultColor = Color.Transparent;
            }

            try
            {
                hex = hex.Replace("#", string.Empty);

                if (hex.Length != 6 && hex.Length != 8) return defaultColor.Value;

                var currentIndex = 0;

                var a = byte.MaxValue;

                if (hex.Length == 8)
                {
                    a = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16);
                    currentIndex += 2;
                }

                var r = (byte)Convert.ToUInt32(hex.Substring(currentIndex, 2), 16);
                currentIndex += 2;

                var g = (byte)Convert.ToUInt32(hex.Substring(currentIndex, 2), 16);
                currentIndex += 2;

                var b = (byte)Convert.ToUInt32(hex.Substring(currentIndex, 2), 16);

                return Color.FromArgb(a, r, g, b);
            }
            catch
            {
                // Return color with default values if available
                return defaultColor.Value;
            }
        }

        /// <summary>
        /// Sets the color's transparency to the specified fraction.
        /// </summary>
        /// <param name="color">Color to apply transparency to</param>
        /// <param name="transparencyFraction">Value from <c>0.0</c> to <c>1.0</c> inclusively</param>
        /// <returns>Color with transparency set to the specified fraction</returns>
        public static Color SetTransparencyFraction(this Color color, double transparencyFraction)
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

            var transparency = (byte)Math.Round(byte.MaxValue * transparencyFraction);

            return Color.FromArgb(transparency, color.R, color.G, color.B);
        }
    }
}
