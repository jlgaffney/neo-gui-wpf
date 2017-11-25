using Autofac;

namespace Neo
{
    public class ApplicationContext : IApplicationContext
    {
        public ILifetimeScope ContainerLifetimeScope { get; set; }
    }
}