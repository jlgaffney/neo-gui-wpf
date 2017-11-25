namespace Neo.Gui.Base.Interfaces
{
    public interface IDialog<TDialogResult>
    {
        object DataContext { get; set; }
        bool? ShowDialog();
    }
}
