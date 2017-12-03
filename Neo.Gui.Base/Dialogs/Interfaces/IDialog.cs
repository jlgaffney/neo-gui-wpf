namespace Neo.Gui.Base.Dialogs.Interfaces
{
    public interface IDialog<TDialogResult>
    {
        object DataContext { get; set; }
        bool? ShowDialog();
    }
}
