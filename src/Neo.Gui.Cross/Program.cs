using System.IO;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Avalonia;
using Avalonia.Logging.Serilog;
using CommonServiceLocator;
using Microsoft.Extensions.Configuration;
using Neo.Gui.Cross.ViewModels;
using Neo.Gui.Cross.Views;
using Neo.Persistence.LevelDB;

namespace Neo.Gui.Cross
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = SetupSettings();

            using (var store = new LevelDBStore(settings.Paths.Chain))
            using (var neoSystem = new NeoSystem(store))
            {
                SetupDependencyInjection(settings, neoSystem);

                BuildAvaloniaApp().Start<ShellWindow>(
                    ViewModelLocator.GetDataContext<ShellWindowViewModel>);
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();

        private static ISettings SetupSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true, reloadOnChange: true);

            return new Settings(builder.Build().GetSection("ApplicationConfiguration"));
        }

        private static void SetupDependencyInjection(ISettings settings, NeoSystem neoSystem)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(neoSystem);
            containerBuilder.RegisterInstance(settings);

            containerBuilder.RegisterModule<AppModule>();

            var container = containerBuilder.Build();

            var cs = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => cs);
        }
    }
}
