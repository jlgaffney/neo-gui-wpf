using Autofac;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Network;

namespace Neo
{
    public interface IApplicationContext
    {
        ILifetimeScope ContainerLifetimeScope { get; set; }

        UserWallet CurrentWallet { get; set; }

        LocalNode LocalNode { get; set; }
    }
}
