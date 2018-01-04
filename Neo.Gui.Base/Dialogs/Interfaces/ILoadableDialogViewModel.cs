namespace Neo.Gui.Base.Dialogs.Interfaces
{
    public interface ILoadableDialogViewModel<TDialogResult, TLoadParameters> : IDialogViewModel<TDialogResult>
    {
        void OnDialogLoad(TLoadParameters parameters);
    }
}
