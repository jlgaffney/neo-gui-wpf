using System;

namespace Neo.Helpers
{
    public interface IDialogViewModel<TDialogResult>
    {
        event EventHandler<TDialogResult> SetDialogResult;

        TDialogResult DialogResult { get; }
    }
}
