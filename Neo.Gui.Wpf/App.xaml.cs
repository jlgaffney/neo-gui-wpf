using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Neo.Controllers;
using Neo.Gui.Base.Interfaces.Helpers;
using Neo.Gui.Wpf.Properties;
using Neo.Helpers;
using Neo.UI;
using Neo.UI.Base;
using Neo.UI.Base.Messages;
using Neo.UI.Home;
using Neo.UI.MarkupExtensions;
using Neo.UI.Messages;
using Neo.UI.Updater;

namespace Neo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private ILifetimeScope containerLifetimeScope;
        private IWalletController walletController;
        
        private App()
        {
            this.InitializeComponent();

            this.Setup();
        }

        private void Setup()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            BuildContainer();

            Task.Run(() =>
            {
                var dispatchHelper = this.containerLifetimeScope?.Resolve<IDispatchHelper>();
                var themeHelper = this.containerLifetimeScope?.Resolve<IThemeHelper>();
                var versionHelper = this.containerLifetimeScope?.Resolve<IVersionHelper>();

                themeHelper?.LoadTheme();

                NeoSplashScreen splashScreen = null;
                dispatchHelper?.InvokeOnMainUIThread(() =>
                {
                    splashScreen = new NeoSplashScreen();
                    splashScreen.Show();
                });

                var updateIsRequired = false;
                try
                {
                    
                    if (versionHelper != null)
                    {
                        updateIsRequired = versionHelper.UpdateIsRequired;
                    }

                    if (updateIsRequired) return;

                    // Application is starting normally, initialize wallet controller
                    this.walletController = this.containerLifetimeScope?.Resolve(typeof(IWalletController)) as IWalletController;

                    this.walletController?.Initialize();
                }
                finally
                {
                    dispatchHelper?.InvokeOnMainUIThread(() =>
                    {
                        this.MainWindow = updateIsRequired ? (Window) new UpdateView() : new HomeView();

                        splashScreen.Close();

                        this.MainWindow?.Show();

                        if (versionHelper != null && !updateIsRequired)
                        {
                            // Check if there is a newer version
                            var latestVersion = versionHelper.LatestVersion;
                            var currentVersion = versionHelper.CurrentVersion;

                            if (latestVersion == null || latestVersion <= currentVersion) return;

                            var messagePublisher = this.containerLifetimeScope?.Resolve(typeof(IMessagePublisher)) as IMessagePublisher;

                            messagePublisher?.Publish(new NewVersionAvailableMessage($"{Strings.DownloadNewVersion}: {latestVersion}"));
                        }
                    });
                }
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Shutdown the wallet controller
            this.walletController.Shutdown();

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
            var lifetimeScope = container.BeginLifetimeScope();

            this.containerLifetimeScope = lifetimeScope;

            SetStaticLifeTimeScopes(lifetimeScope);
        }

        private static void SetStaticLifeTimeScopes(ILifetimeScope containerLifetimeScope)
        {
            DialogHelper.SetLifetimeScope(containerLifetimeScope);
            DataContextBindingExtension.SetLifetimeScope(containerLifetimeScope);
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