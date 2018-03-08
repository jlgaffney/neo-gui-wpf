namespace Neo.UniversalWallet.ViewModels.Helpers
{
    public interface IView
    {
        object Tag { get; set; }

        object DataContext { get; set; }
    }
}
