using System;

namespace Neo.Gui.Base.Dialogs.Interfaces
{
    public interface IDialogViewModel<TLoadParameters>
    {
        event EventHandler Close;

        void OnDialogLoad(TLoadParameters parameters);
    }
}