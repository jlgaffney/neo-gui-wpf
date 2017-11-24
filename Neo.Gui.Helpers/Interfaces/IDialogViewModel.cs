using System;

namespace Neo.Gui.Helpers.Interfaces
{
    public interface IDialogViewModel<TDialogResult>
    {
        event EventHandler<TDialogResult> SetDialogResult;

        TDialogResult DialogResult { get; }
    }
}