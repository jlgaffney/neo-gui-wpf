using Autofac;

namespace Neo.Gui.Wpf
{
    public interface IApplicationContext
    {
        ILifetimeScope ContainerLifetimeScope { get; set; }
    }
}