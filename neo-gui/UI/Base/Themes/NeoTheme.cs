using System.Windows.Media;
using Newtonsoft.Json;

namespace Neo.UI.Base.Themes
{
    public class NeoTheme
    {
        public static NeoTheme Current { get; private set; }

        public ThemeStyle Style { get; set; }

        public Color AccentBaseColor { get; set; }

        public Color HighlightColor { get; set; }

        public Color WindowBorderColor { get; set; }

        public void SetAsCurrentTheme()
        {
            ThemeHelper.SetTheme(this);

            // Update static current theme property
            Current = this;
        }
        
        public static NeoTheme Import(string themeJson)
        {
            if (string.IsNullOrEmpty(themeJson)) return null;

            NeoTheme theme;
            try
            {
                theme = JsonConvert.DeserializeObject<NeoTheme>(themeJson);
            }
            catch
            {
                // Invalid format
                return null;
            }

            return theme;
        }

        public static string Export(NeoTheme theme)
        {
            return JsonConvert.SerializeObject(theme, Formatting.Indented);
        }
    }
}