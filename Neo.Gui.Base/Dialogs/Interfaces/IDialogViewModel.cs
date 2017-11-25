using System;

namespace Neo.Gui.Base.Dialogs.Interfaces
{
    public interface IDialogViewModel<TDialogResult>
    {
        event EventHandler<TDialogResult> SetDialogResult;
        TDialogResult DialogResult { get; }
    }
}