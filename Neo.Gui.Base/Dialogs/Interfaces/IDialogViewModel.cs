using System;

namespace Neo.Gui.Base.Dialogs.Interfaces
{
    public interface IDialogViewModel<TDialogResult>
    {
        event EventHandler Close;

        event EventHandler<TDialogResult> SetDialogResultAndClose;

        TDialogResult DialogResult { get; }
    }
}