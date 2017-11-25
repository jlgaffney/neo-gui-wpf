using Neo.Gui.Base.Theming;

namespace Neo.Gui.Base.Interfaces.Helpers
{
    public interface IThemeHelper
    {
        NeoGuiTheme CurrentTheme { get; }

        void LoadTheme();

        void SetTheme(NeoGuiTheme newTheme);
    }
}
