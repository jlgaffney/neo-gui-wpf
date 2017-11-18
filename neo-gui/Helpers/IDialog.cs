namespace Neo.Helpers
{
    public interface IDialog<TDialogResult>
    {
        object DataContext { get; set; }

        bool? ShowDialog();
    }
}
