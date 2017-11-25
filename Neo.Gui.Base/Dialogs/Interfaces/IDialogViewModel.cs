using System;

namespace Neo.Gui.Base.Interfaces
{
    public interface IDialogViewModel<TDialogResult>
    {
        event EventHandler<TDialogResult> SetDialogResult;
        TDialogResult DialogResult { get; }
    }
}