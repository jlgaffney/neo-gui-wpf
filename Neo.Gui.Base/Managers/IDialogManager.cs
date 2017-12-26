using System;

using Neo.Gui.Base.Dialogs.Interfaces;

namespace Neo.Gui.Base.Managers
{
    /// <summary>
    /// Interface that abstracts the usage of dialog views in the application.
    /// </summary>
    public interface IDialogManager
    {
        IDialog<TDialogResult> CreateDialog<TDialogResult, TLoadParameters>(Action<TDialogResult> resultSetter, TLoadParameters parameters);
        
        IDialog<TDialogResult> CreateDialog<TDialogResult>(Action<TDialogResult> resultSetter);

        TDialogResult ShowDialog<TDialogResult>();

        TDialogResult ShowDialog<TDialogResult, TLoadParameters>(TLoadParameters parameters);

        string ShowInputDialog(string title, string message, string input = "");
        
        void ShowInformationDialog(string title, string message, string text);

        MessageDialogResult ShowMessageDialog(string title, string message, MessageDialogType type = MessageDialogType.Ok, MessageDialogResult defaultResult = MessageDialogResult.Ok);
    }
}