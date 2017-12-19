using System;
using System.Drawing;

namespace Neo.Gui.Base.Extensions
{
    public static class ColorExtensions
    {
        public static string ToHex(this Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
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
