using System.Drawing;
using Neo.Gui.Base.Extensions;
using Newtonsoft.Json;

namespace Neo.Gui.Base.Theming
{
    public class NeoGuiTheme
    {
        public static readonly NeoGuiTheme Default = new NeoGuiTheme
        {
            Style = ThemeStyle.Light,
            HighlightColor = "#76B466".HexToColor(),
            AccentBaseColor = "#3DA43C".HexToColor(),
            WindowBorderColor = "#9EAF99".HexToColor()
        };
        
        public ThemeStyle Style { get; set; }

        public Color AccentBaseColor { get; set; }

        public Color HighlightColor { get; set; }

        public Color WindowBorderColor { get; set; }
        
        public static NeoGuiTheme ImportFromJson(string themeJson)
        {
            if (string.IsNullOrEmpty(themeJson)) return null;

            NeoGuiTheme theme;
            try
            {
                theme = JsonConvert.DeserializeObject<NeoGuiTheme>(themeJson);
            }
            catch
            {
                // Invalid format
                return null;
            }

            return theme;
        }

        public static string ExportToJson(NeoGuiTheme theme)
        {
            return JsonConvert.SerializeObject(theme, Formatting.Indented);
        }
    }
}