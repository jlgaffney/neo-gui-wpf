using Autofac;
using Neo.Implementations.Wallets.EntityFramework;

namespace Neo
{
    public interface IApplicationContext
    {
        ILifetimeScope ContainerLifetimeScope { get; set; }

        UserWallet CurrentWallet { get; set; }
    }
}
