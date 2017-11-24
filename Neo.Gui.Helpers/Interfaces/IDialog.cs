namespace Neo.Gui.Helpers.Interfaces
{
    public interface IDialog<TDialogResult>
    {
        object DataContext { get; set; }

        bool? ShowDialog();
    }
}