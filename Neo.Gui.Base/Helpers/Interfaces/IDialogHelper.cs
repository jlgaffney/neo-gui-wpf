using Neo.Gui.Base.MVVM;

namespace Neo.Gui.Base.Helpers.Interfaces
{
    /// <summary>
    /// Interface of the DialogHelper that abstracts the usage of Dialog windows in the application.
    /// </summary>
    public interface IDialogHelper
    {
        TDialogResult ShowDialog<TDialogResult>();

        TDialogResult ShowDialog<TDialogResult, TLoadParameters>(ILoadParameters<TLoadParameters> parameters);
    }
}