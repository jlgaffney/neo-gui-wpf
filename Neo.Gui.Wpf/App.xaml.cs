using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

using Autofac;

using Neo.Gui.Globalization.Resources;

using Neo.Gui.Base;
using Neo.Gui.Dialogs.LoadParameters.Home;
using Neo.Gui.Dialogs.LoadParameters.Updater;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.Wpf.Controls;
using Neo.Gui.Wpf.Extensions;
using Neo.Gui.Wpf.Implementations.Managers;
using Neo.Gui.Wpf.MarkupExtensions;
using Neo.Gui.Wpf.Properties;
using Neo.Gui.Wpf.RegistrationModules;
using Neo.UI.Core;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using SplashScreen = Neo.Gui.Wpf.Views.SplashScreen;
using ViewModelsRegistrationModule = Neo.Gui.ViewModels.ViewModelsRegistrationModule;
using WpfProjectViewModelsRegistrationModule = Neo.Gui.Wpf.RegistrationModules.ViewModelsRegistrationModule;

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

            this.Initialize();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose of controller instances
            this.walletController.Dispose();
            this.walletController = null;

            base.OnExit(e);
        }

        private void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var containerLifetimeScope = BuildContainer();

            Debug.Assert(containerLifetimeScope != null);

            // Set static lifetime scopes
            DialogManager.SetLifetimeScope(containerLifetimeScope);
            DataContextBindingExtension.SetLifetimeScope(containerLifetimeScope);

            var dialogManager = containerLifetimeScope.Resolve<IDialogManager>() as DialogManager;
            var dispatchService = containerLifetimeScope.Resolve<IDispatchService>();
            var themeManager = containerLifetimeScope.Resolve<IThemeManager>();
            var versionHelper = containerLifetimeScope.Resolve<IVersionService>();
            this.walletController = containerLifetimeScope.Resolve<IWalletController>();

            Debug.Assert(
                dialogManager != null &&
                dispatchService != null &&
                themeManager != null &&
                versionHelper != null &&
                this.walletController != null);

            TransactionOutputListBox.SetDialogManager(dialogManager);

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
                try
                {
                    if (versionHelper.UpdateIsRequired)
                    {
                        // Display update window
                        await dispatchService.InvokeOnMainUIThread(() =>
                        {
                            window = dialogManager.CreateDialog<UpdateLoadParameters>(null) as Window;
                        });
                        return;
                    }

                    // Application is starting normally

                    // Initialize wallet controller
                    this.walletController.Initialize();
                    this.walletController.SetNEP5WatchScriptHashes(Settings.Default.NEP5Watched.ToArray());

                    await dispatchService.InvokeOnMainUIThread(() =>
                    {
                        window = dialogManager.CreateDialog<HomeLoadParameters>(null) as Window;
                    });
                }
                finally
                {
                    Debug.Assert(window != null);

                    await dispatchService.InvokeOnMainUIThread(() =>
                    {
                        this.MainWindow = window;
                        this.MainWindow?.Show();
                        
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
                Process.Start(new ProcessStartInfo
                {
                    FileName = Assembly.GetExecutingAssembly().Location,
                    UseShellExecute = true,
                    Verb = "runas",
                    WorkingDirectory = Environment.CurrentDirectory
                });

                // Stop this application instance
                Current.Shutdown();
            }
            catch (Win32Exception)
            {
                // TODO Log exception somewhere
            }
        }

        private static ILifetimeScope BuildContainer()
        {
            var autoFacContainerBuilder = new ContainerBuilder();

            autoFacContainerBuilder.RegisterModule<CoreRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<WpfProjectViewModelsRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ViewModelsRegistrationModule>();
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
    }
}