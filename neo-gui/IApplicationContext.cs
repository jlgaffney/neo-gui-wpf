using Autofac;

namespace Neo
{
    public interface IApplicationContext
    {
        ILifetimeScope ContainerLifetimeScope { get; set; }
    }
}