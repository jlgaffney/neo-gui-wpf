namespace Neo.Gui.Dialogs.Interfaces
{
    public interface IDialog<TLoadParameters>
    {
        object DataContext { get; set; }
        bool? ShowDialog();
    }
}
