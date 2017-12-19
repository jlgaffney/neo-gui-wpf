using System;
using System.Drawing;

namespace Neo.Gui.Base.Extensions
{
    public static class StringExtensions
    {
        public static string[] ToLines(this string source)
        {
            if (string.IsNullOrEmpty(source)) return new string[0];

            var lines = source.Split('\n');
            
            // Remove \r character from end of line if present
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (line[line.Length - 1] == '\r')
                {
                    line = line.Substring(0, line.Length - 1);
                }

                lines[i] = line;
            }

            return lines;
        }

        public static string ToMultiLineString(this string[] source)
        {
            var value = string.Empty;

            for (int i = 0; i < source.Length; i++)
            {
                value += source[i];

                if (i >= source.Length - 1) continue;

                // Append new line characters
                value += Environment.NewLine;
            }

            return value;
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
                
                var r = (byte) Convert.ToUInt32(hex.Substring(currentIndex, 2), 16);
                currentIndex += 2;

                var g = (byte) Convert.ToUInt32(hex.Substring(currentIndex, 2), 16);
                currentIndex += 2;

                var b = (byte) Convert.ToUInt32(hex.Substring(currentIndex, 2), 16);

                return Color.FromArgb(a, r, g, b);
            }
            catch
            {
                // Return color with default values if available
                return defaultColor.Value;
            }
        }
    }
}