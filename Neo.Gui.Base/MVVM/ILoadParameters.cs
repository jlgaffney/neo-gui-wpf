namespace Neo.Gui.Base.MVVM
{ 
    public interface ILoadParameters<TParameterObject>
    {
        TParameterObject Parameters { get; }
    }
}
