using System.Drawing;
using Neo.UI.Core.Extensions;
using Newtonsoft.Json;

namespace Neo.UI.Core.Theming
{
    public class Theme
    {
        // TODO Add default theme colors to a config file so the default values can be changed by the user
        private const string DefaultHighlightColorHex = "#76B466";
        private const string DefaultAccentBaseColorHex = "#3DA43C";
        private const string DefaultWindowBorderColorHex = "#9EAF99";

        public static readonly Theme Default = new Theme
        {
            Style = Style.Light,
            HighlightColor = DefaultHighlightColorHex.HexToColor(),
            AccentBaseColor = DefaultAccentBaseColorHex.HexToColor(),
            WindowBorderColor = DefaultWindowBorderColorHex.HexToColor()
        };
        
        public Style Style { get; set; }

        public Color AccentBaseColor { get; set; }

        public Color HighlightColor { get; set; }

        public Color WindowBorderColor { get; set; }
        
        public static Theme ImportFromJson(string themeJson)
        {
            if (string.IsNullOrEmpty(themeJson)) return null;

            Theme theme;
            try
            {
                theme = JsonConvert.DeserializeObject<Theme>(themeJson);
            }
            catch
            {
                // Invalid format
                return null;
            }

            return theme;
        }

        public static string ExportToJson(Theme theme)
        {
            return JsonConvert.SerializeObject(theme, Formatting.Indented);
        }
    }
}