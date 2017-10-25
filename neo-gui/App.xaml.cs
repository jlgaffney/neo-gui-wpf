using System.Windows;
using Autofac;
using Neo.UI;
using Neo.UI.Base.Themes;

namespace Neo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        internal App()
        {
            this.InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var autoFacContainerBuilder = new ContainerBuilder();
            autoFacContainerBuilder.RegisterModule<ViewModelsRegistrationModule>();

            var container = autoFacContainerBuilder.Build();
            ApplicationContext.Instance.ContainerLifetimeScope = container.BeginLifetimeScope();

            ThemeHelper.LoadTheme();

            base.OnStartup(e);
        }
    }
}