using System;

namespace Neo.Gui.Dialogs.Interfaces
{
    public interface IResultDialogViewModel<TLoadParameters, TDialogResult> : IDialogViewModel<TLoadParameters>
    {
        event EventHandler<TDialogResult> SetDialogResultAndClose;
    }
}
