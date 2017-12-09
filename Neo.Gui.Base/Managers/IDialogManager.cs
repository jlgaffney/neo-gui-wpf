using Neo.Gui.Base.MVVM;

namespace Neo.Gui.Base.Managers
{
    /// <summary>
    /// Interface of the DialogManager that abstracts the usage of Dialog windows in the application.
    /// </summary>
    public interface IDialogManager
    {
        TDialogResult ShowDialog<TDialogResult>();

        TDialogResult ShowDialog<TDialogResult, TLoadParameters>(ILoadParameters<TLoadParameters> parameters);

        MessageDialogResult ShowMessage(string title, string message, MessageDialogType type = MessageDialogType.Ok, MessageDialogResult defaultResult = MessageDialogResult.Ok);
    }
}