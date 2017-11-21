using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Neo.Controllers;
using Neo.Helpers;
using Neo.UI;
using Neo.UI.Base;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.Themes;
using Neo.UI.Home;
using Neo.UI.Updater;

namespace Neo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IBlockChainController blockChainController;
        private IApplicationContext applicationContext;

        private App()
        {
            this.InitializeComponent();

            this.Setup();
        }

        private void Setup()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            BuildContainer();

            var dispatcher = this.applicationContext.ContainerLifetimeScope.Resolve(typeof(IDispatcher)) as IDispatcher;

            Task.Run(() =>
            {
                ThemeHelper.LoadTheme();

                NeoSplashScreen splashScreen = null;
                dispatcher?.InvokeOnMainUIThread(() =>
                {
                    splashScreen = new NeoSplashScreen();
                    splashScreen.Show();
                });

                var updateIsRequired = false;
                try
                {
                    updateIsRequired = VersionHelper.UpdateIsRequired;

                    if (updateIsRequired) return;

                    // Application is starting normally, setup blockchain
                    this.blockChainController =
                        this.applicationContext.ContainerLifetimeScope.Resolve(typeof(IBlockChainController)) as
                            IBlockChainController;

                    this.blockChainController?.Setup();
                }
                finally
                {
                    dispatcher?.InvokeOnMainUIThread(() =>
                    {
                        this.MainWindow = updateIsRequired ? (Window) new UpdateView() : new HomeView();

                        splashScreen.Close();

                        this.MainWindow?.Show();
                    });
                }
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose of IBlockChainController instance
            this.blockChainController.Dispose();
            this.blockChainController = null;

            base.OnExit(e);
        }

        private void BuildContainer()
        {
            var autoFacContainerBuilder = new ContainerBuilder();

            autoFacContainerBuilder.RegisterModule<NeoGuiRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ViewModelsRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<BaseRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ControllersRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<HelpersRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<DialogsRegistrationModule>();

            var container = autoFacContainerBuilder.Build();

            this.applicationContext = container.Resolve<IApplicationContext>();
            this.applicationContext.ContainerLifetimeScope = container.BeginLifetimeScope();
            ApplicationContext.Instance.ContainerLifetimeScope = container.BeginLifetimeScope();
        }

        #region Unhandled exception methods

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using (var fileStream = new FileStream("error.log", FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(fileStream))
            {
                PrintErrorLogs(writer, (Exception)e.ExceptionObject);
            }
        }

        private static void PrintErrorLogs(StreamWriter writer, Exception ex)
        {
            writer.WriteLine(ex.GetType());
            writer.WriteLine(ex.Message);
            writer.WriteLine(ex.StackTrace);

            // Print inner exceptions if there are any
            if (ex is AggregateException ex2)
            {
                foreach (var innerException in ex2.InnerExceptions)
                {
                    writer.WriteLine();
                    PrintErrorLogs(writer, innerException);
                }
            }
            else if (ex.InnerException != null)
            {
                writer.WriteLine();
                PrintErrorLogs(writer, ex.InnerException);
            }
        }

        #endregion
    }
}