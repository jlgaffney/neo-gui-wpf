using Autofac;
using Neo.Network;

namespace Neo
{
    public interface IApplicationContext
    {
        ILifetimeScope ContainerLifetimeScope { get; set; }
        
        LocalNode LocalNode { get; set; }
    }
}