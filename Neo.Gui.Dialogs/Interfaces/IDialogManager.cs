using Neo.Gui.Dialogs;
using Neo.Gui.Dialogs.Interfaces;

namespace Neo.Gui.Base.Managers.Interfaces
{
    /// <summary>
    /// Interface that abstracts the usage of dialog views in the application.
    /// </summary>
    public interface IDialogManager
    {
        IDialog<TLoadParameters> CreateDialog<TLoadParameters>(TLoadParameters parameters);

        void ShowDialog<TLoadParameters>(TLoadParameters parameters = default(TLoadParameters));

        TDialogResult ShowDialog<TLoadParameters, TDialogResult>(TLoadParameters parameters = default(TLoadParameters));

        string ShowInputDialog(string title, string message, string input = "");
        
        void ShowInformationDialog(string title, string message, string text);

        MessageDialogResult ShowMessageDialog(string title, string message, MessageDialogType type = MessageDialogType.Ok, MessageDialogResult defaultResult = MessageDialogResult.Ok);
    }
}