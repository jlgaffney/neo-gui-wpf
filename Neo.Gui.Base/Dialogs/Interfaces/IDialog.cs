namespace Neo.Gui.Base.Dialogs.Interfaces
{
    public interface IDialog<TLoadParameters>
    {
        object DataContext { get; set; }
        bool? ShowDialog();
    }
}
