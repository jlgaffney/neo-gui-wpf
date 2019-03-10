namespace Neo.Gui.Cross.ViewModels
{
    public interface ILoadable
    {
        void Load();
    }

    public interface ILoadable<TLoadParameters>
    {
        void Load(TLoadParameters parameters);
    }
}
