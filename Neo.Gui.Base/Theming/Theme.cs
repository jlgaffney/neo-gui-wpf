using System.Drawing;
using Neo.Gui.Base.Extensions;
using Newtonsoft.Json;

namespace Neo.Gui.Base.Theming
{
    public class Theme
    {
        public static readonly Theme Default = new Theme
        {
            Style = Style.Light,
            HighlightColor = "#76B466".HexToColor(),
            AccentBaseColor = "#3DA43C".HexToColor(),
            WindowBorderColor = "#9EAF99".HexToColor()
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