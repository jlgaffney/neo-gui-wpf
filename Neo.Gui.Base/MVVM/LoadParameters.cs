namespace Neo.Gui.Base.MVVM
{
    public class LoadParameters<TParametersObject> : ILoadParameters<TParametersObject>
    {
        public TParametersObject Parameters { get; }

        public LoadParameters(TParametersObject parameters)
        {
            this.Parameters = parameters;
        }
    }
}
