using System;
using Autofac;

namespace Neo
{
    public class ApplicationContext : IApplicationContext
    {
        #region Singleton Pattern
        private static readonly Lazy<ApplicationContext> lazyInstance = new Lazy<ApplicationContext>(() => new ApplicationContext());

        public static ApplicationContext Instance => lazyInstance.Value;
        #endregion

        public ILifetimeScope ContainerLifetimeScope { get; set; }
    }
}