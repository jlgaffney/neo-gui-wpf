namespace Neo.UI.MVVM
{
    public interface IHandle { }
    public interface IHandle<T> : IHandle
    {
        void Handle(T message);
    }
}
