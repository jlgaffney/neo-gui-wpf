namespace Neo.UI.Base.MVVM
{
    public interface IHandle { }
    public interface IHandle<T> : IHandle
    {
        void Handle(T message);
    }
}
