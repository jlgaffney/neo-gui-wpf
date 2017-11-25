using Autofac;

namespace Neo.Gui.Wpf
{
    public class ApplicationContext : IApplicationContext
    {
        public ILifetimeScope ContainerLifetimeScope { get; set; }
    }
}