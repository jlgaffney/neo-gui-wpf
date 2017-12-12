using Neo.Gui.Base.Theming;

namespace Neo.Gui.Base.Managers
{
    public interface IThemeManager
    {
        Theme CurrentTheme { get; }

        void LoadTheme();

        void SetTheme(Theme newTheme);
    }
}
