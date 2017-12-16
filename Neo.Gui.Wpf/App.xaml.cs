using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

using Autofac;

using Neo.Gui.Base;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Results.Home;
using Neo.Gui.Base.Dialogs.Results.Settings;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Services;

using Neo.Gui.Wpf.Extensions;
using Neo.Gui.Wpf.Implementations.Managers;
using Neo.Gui.Wpf.MarkupExtensions;
using Neo.Gui.Wpf.Properties;
using Neo.Gui.Wpf.RegistrationModules;
using SplashScreen = Neo.Gui.Wpf.Views.SplashScreen;
using ViewModelsRegistrationModule = Neo.Gui.ViewModels.ViewModelsRegistrationModule;
using WpfProjectViewModelsRegistrationModule = Neo.Gui.Wpf.RegistrationModules.ViewModelsRegistrationModule;

namespace Neo.Gui.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IMessageHandler<ExitAppMessage>
    {
        private IWalletController walletController;
        
        private App()
        {
            this.InitializeComponent();

            this.Setup();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose of controller instances
            this.walletController.Dispose();
            this.walletController = null;

            base.OnExit(e);
        }

        private void Setup()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var containerLifetimeScope = BuildContainer();

            Debug.Assert(containerLifetimeScope != null);

            // Set static lifetime scopes
            DialogManager.SetLifetimeScope(containerLifetimeScope);
            DataContextBindingExtension.SetLifetimeScope(containerLifetimeScope);

            var dialogManager = containerLifetimeScope.Resolve<IDialogManager>() as DialogManager;
            var dispatchService = containerLifetimeScope.Resolve<IDispatchService>();
            var messagePublisher = containerLifetimeScope.Resolve<IMessagePublisher>();
            var messageSubscriber = containerLifetimeScope.Resolve<IMessageSubscriber>();
            var themeManager = containerLifetimeScope.Resolve<IThemeManager>();
            var versionHelper = containerLifetimeScope.Resolve<IVersionHelper>();
            this.walletController = containerLifetimeScope.Resolve<IWalletController>();

            Debug.Assert(
                dialogManager != null &&
                dispatchService != null &&
                messagePublisher != null &&
                messageSubscriber != null &&
                themeManager != null &&
                versionHelper != null &&
                this.walletController != null);

            messageSubscriber.Subscribe(this);

            Task.Run(async () =>
            {
                InstallRootCertificateIfRequired();

                themeManager.LoadTheme();

                // Show splash screen
                SplashScreen splashScreen = null;
                await dispatchService.InvokeOnMainUIThread(() =>
                {
                    splashScreen = new SplashScreen();
                    splashScreen.Show();
                });

                Window window = null;
                Version newerVersion = null;
                try
                {
                    if (versionHelper.UpdateIsRequired)
                    {
                        // Display update window
                        await dispatchService.InvokeOnMainUIThread(() =>
                        {
                            window = dialogManager.CreateDialog<UpdateDialogResult>(result => {  }) as Window;
                        });
                        return;
                    }

                    // Application is starting normally

                    // Initialize wallet controller
                    this.walletController.Initialize(Settings.Default.Paths.CertCache);
                    this.walletController.SetNEP5WatchScriptHashes(Settings.Default.NEP5Watched.ToArray());

                    // Check if there a newer version is available
                    var latestVersion = versionHelper.LatestVersion;
                    var currentVersion = versionHelper.CurrentVersion;
                    if (latestVersion != null && currentVersion != null && latestVersion > currentVersion)
                    {
                        newerVersion = latestVersion;
                    }

                    await dispatchService.InvokeOnMainUIThread(() =>
                    {
                        window = dialogManager.CreateDialog<HomeDialogResult>(result => { }) as Window;
                    });
                }
                finally
                {
                    Debug.Assert(window != null);

                    await dispatchService.InvokeOnMainUIThread(() =>
                    {
                        // Load this.MainWindow.DataContext if required
                        var loadableDataContext = window.DataContext as ILoadable;
                        loadableDataContext?.OnLoad();

                        this.MainWindow = window;
                        this.MainWindow?.Show();
                        
                        if (newerVersion != null)
                        {
                            messagePublisher.Publish(new NewVersionAvailableMessage(newerVersion));
                        }
                        
                        // Close splash screen
                        splashScreen.Close();
                    });
                }
            });
        }

        private static void InstallRootCertificateIfRequired()
        {
            // Only install if using a local node
            // TODO Is the root certificate required if connecting to a remote node?
            if (Settings.Default.P2P.RemoteNodeMode) return;

            if (!Settings.Default.InstallCertificate) return;

            // Check if root certificate is already installed
            if (RootCertificate.IsInstalled) return;

            // Confirm with user before trying to install root certificate
            if (MessageBox.Show(Strings.InstallCertificateText, Strings.InstallCertificateCaption,
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) != MessageBoxResult.Yes)
            {
                // User has chosen not to install the Onchain root certificate
                Settings.Default.InstallCertificate = false;
                Settings.Default.Save();
                return;
            }

            // Try install root certificate
            var certificateInstalled = RootCertificate.Install();

            if (certificateInstalled) return;

            var runAsAdmin = MessageBox.Show(
                "Onchain root certificate could not be installed! Do you want to try running the application as adminstrator?",
                "Root certificate installation failed!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                MessageBoxResult.No);

            if (runAsAdmin != MessageBoxResult.Yes) return;

            // Try running application as administrator to install root certificate
            try
            {
                // TODO Stop this application instance

                Process.Start(new ProcessStartInfo
                {
                    FileName = Assembly.GetExecutingAssembly().Location,
                    UseShellExecute = true,
                    Verb = "runas",
                    WorkingDirectory = Environment.CurrentDirectory
                });
            }
            catch (Win32Exception)
            {
                // TODO Log exception somewhere
            }
        }

        private static ILifetimeScope BuildContainer()
        {
            var autoFacContainerBuilder = new ContainerBuilder();

            autoFacContainerBuilder.RegisterModule<BaseRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<WpfProjectViewModelsRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ViewModelsRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<MessagingRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ControllersRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<HelpersRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ManagersRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ServicesRegistrationModule>();
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

        #region MessageHandler implementation 
        public void HandleMessage(ExitAppMessage message)
        {
            Current.Shutdown();
        }
        #endregion
    }
}