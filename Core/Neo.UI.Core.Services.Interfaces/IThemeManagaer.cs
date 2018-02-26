using Neo.UI.Core.Theming;

namespace Neo.UI.Core.Services.Interfaces
{
    public interface IThemeManager
    {
        Theme CurrentTheme { get; }

        void LoadTheme();

        void SetTheme(Theme newTheme);
    }
}
