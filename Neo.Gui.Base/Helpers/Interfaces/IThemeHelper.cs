using Neo.Gui.Base.Theming;

namespace Neo.Gui.Base.Helpers.Interfaces
{
    public interface IThemeHelper
    {
        Theme CurrentTheme { get; }

        void LoadTheme();

        void SetTheme(Theme newTheme);
    }
}
