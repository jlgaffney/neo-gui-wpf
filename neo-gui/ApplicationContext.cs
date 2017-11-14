using System;
using Autofac;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Network;

namespace Neo
{
    public class ApplicationContext : IApplicationContext
    {
        #region Singleton Pattern
        private static readonly Lazy<ApplicationContext> lazyInstance = new Lazy<ApplicationContext>(() => new ApplicationContext());

        // Prevent other classes from calling constructor
        private ApplicationContext()
        {
        }

        public static ApplicationContext Instance => lazyInstance.Value;

        #endregion

        public ILifetimeScope ContainerLifetimeScope { get; set; }

        public UserWallet CurrentWallet { get; set; }

        public LocalNode LocalNode { get; set; }
    }
}