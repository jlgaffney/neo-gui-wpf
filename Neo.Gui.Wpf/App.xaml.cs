using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

using Autofac;

using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Home;
using Neo.Gui.Dialogs.LoadParameters.Updater;

using Neo.Gui.Wpf.Controls;
using Neo.Gui.Wpf.Extensions;
using Neo.Gui.Wpf.MarkupExtensions;
using Neo.Gui.Wpf.Native;
using Neo.Gui.Wpf.Native.Services;
using Neo.Gui.Wpf.Properties;
using Neo.Gui.Wpf.Views;

using Neo.UI.Core.Globalization.Resources;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Wallet;
using Neo.UI.Core.Wallet.Initialization;
using SplashScreen = Neo.Gui.Wpf.Views.SplashScreen;
using ViewModelsRegistrationModule = Neo.Gui.ViewModels.ViewModelsRegistrationModule;
using WpfProjectViewModelsRegistrationModule = Neo.Gui.Wpf.Views.ViewModelsRegistrationModule;

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

            var lightMode = Settings.Default.LightWalletMode;

            var containerLifetimeScope = BuildContainer(lightMode);

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
                    IWalletInitializationParameters initializationParameters;
                    if (lightMode)
                    {
                        initializationParameters = new LightWalletInitializationParameters(
                            Settings.Default.LightWallet.RpcSeedList,
                            Settings.Default.Paths.CertCache);
                    }
                    else
                    {
                        initializationParameters = new FullWalletInitializationParameters(
                            Settings.Default.P2P.Port, Settings.Default.P2P.WsPort,
                            Settings.Default.Paths.Chain, Settings.Default.Paths.CertCache);
                    }

                    this.walletController.Initialize(initializationParameters);
                    this.walletController.SetNEP5WatchScriptHashes(Settings.Default.NEP5Watched.ToArray());

                    await dispatchService.InvokeOnMainUIThread(() =>
                    {
                        window = dialogManager.CreateDialog<HomeLoadParameters>(null) as Window;
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
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

        private static ILifetimeScope BuildContainer(bool lightMode)
        {
            var autoFacContainerBuilder = new ContainerBuilder();

            WalletRegistrationModule.LightMode = lightMode;
            autoFacContainerBuilder.RegisterModule<WalletRegistrationModule>();

            autoFacContainerBuilder.RegisterModule<NativeServicesRegistrationModule>();

            autoFacContainerBuilder.RegisterModule<WpfProjectViewModelsRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ViewModelsRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<DialogsRegistrationModule>();

            var container = autoFacContainerBuilder.Build();
            var lifetimeScope = container.BeginLifetimeScope();

            return lifetimeScope;
        }

        private static void InstallRootCertificateIfRequired()
        {
            // Only install if using a local node
            // TODO Is the root certificate required if connecting to a remote node?
            if (Settings.Default.LightWalletMode) return;

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