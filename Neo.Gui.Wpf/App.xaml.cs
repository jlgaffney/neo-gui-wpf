using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Neo.Gui.Base;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Wpf.Certificates;
using Neo.Gui.Wpf.Extensions;
using Neo.Gui.Wpf.Helpers;
using Neo.Gui.Wpf.MarkupExtensions;
using Neo.Gui.Wpf.RegistrationModules;
using Neo.Gui.Wpf.Views.Home;
using Neo.Gui.Wpf.Views.Updater;
using Settings = Neo.Gui.Wpf.Properties.Settings;
using SplashScreen = Neo.Gui.Wpf.Views.SplashScreen;

namespace Neo.Gui.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IWalletController walletController;
        
        private App()
        {
            this.InitializeComponent();

            this.Setup();
        }

        private void Setup()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var containerLifetimeScope = BuildContainer();

            Debug.Assert(containerLifetimeScope != null);

            // Set static lifetime scopes
            DialogHelper.SetLifetimeScope(containerLifetimeScope);
            DataContextBindingExtension.SetLifetimeScope(containerLifetimeScope);

            var dispatchHelper = containerLifetimeScope.Resolve<IDispatchHelper>();
            var themeHelper = containerLifetimeScope.Resolve<IThemeHelper>();
            var versionHelper = containerLifetimeScope.Resolve<IVersionHelper>();

            Debug.Assert(dispatchHelper != null);

            Task.Run(() =>
            {
                themeHelper?.LoadTheme();

                SplashScreen splashScreen = null;
                dispatchHelper.InvokeOnMainUIThread(() =>
                {
                    splashScreen = new SplashScreen();
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

                    // Application is starting normally, initialize controllers
                    this.walletController = containerLifetimeScope.Resolve<IWalletController>();

                    Debug.Assert(this.walletController != null);

                    if (!Settings.Default.RemoteNodeMode)
                    {
                        // Local node is being used, install root certificate
                        if (!RootCertificate.Install()) return;
                    }

                    this.walletController.Initialize(Settings.Default.CertCachePath);

                    this.walletController.SetNEP5WatchScriptHashes(Settings.Default.NEP5Watched.ToArray());
                }
                finally
                {
                    dispatchHelper.InvokeOnMainUIThread(() =>
                    {
                        this.MainWindow = updateIsRequired ? (Window) new UpdateView() : new HomeView();

                        splashScreen.Close();

                        this.MainWindow?.Show();

                        if (versionHelper == null || updateIsRequired) return;

                        // Check if there is a newer version
                        var latestVersion = versionHelper.LatestVersion;
                        var currentVersion = versionHelper.CurrentVersion;

                        if (latestVersion == null || latestVersion <= currentVersion) return;

                        var messagePublisher = containerLifetimeScope.Resolve(typeof(IMessagePublisher)) as IMessagePublisher;

                        messagePublisher?.Publish(new NewVersionAvailableMessage($"{Strings.DownloadNewVersion}: {latestVersion}"));
                    });
                }
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose of controller instances
            this.walletController.Dispose();
            this.walletController = null;

            base.OnExit(e);
        }

        private static ILifetimeScope BuildContainer()
        {
            var autoFacContainerBuilder = new ContainerBuilder();

            autoFacContainerBuilder.RegisterModule<BaseRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ViewModelsRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<MessagingRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ControllersRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<HelpersRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<DialogsRegistrationModule>();

            var container = autoFacContainerBuilder.Build();
            var lifetimeScope = container.BeginLifetimeScope();

            return lifetimeScope;
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

        private static void PrintErrorLogs(TextWriter writer, Exception ex)
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