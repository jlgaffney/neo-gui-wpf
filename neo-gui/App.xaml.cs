﻿using System.Windows;
using Autofac;
using Neo.Controllers;
using Neo.Helpers;
using Neo.UI;
using Neo.UI.Base;
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
        internal App(bool updateIsRequired = false)
        {
            this.InitializeComponent();

            BuildContainer();

            var blockChainController = ApplicationContext.Instance.ContainerLifetimeScope.Resolve(typeof(IBlockChainController)) as IBlockChainController;
            blockChainController.StartLocalNode();

            this.MainWindow = updateIsRequired ? (Window)new UpdateView() : new HomeView();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeHelper.LoadTheme();

            this.MainWindow?.Show();

            base.OnStartup(e);
        }

        private static void BuildContainer()
        {
            var autoFacContainerBuilder = new ContainerBuilder();

            autoFacContainerBuilder.RegisterModule<NeoGuiRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ViewModelsRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<BaseRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<ControllersRegistrationModule>();
            autoFacContainerBuilder.RegisterModule<HelpersRegistrationModule>();

            var container = autoFacContainerBuilder.Build();

            ApplicationContext.Instance.ContainerLifetimeScope = container.BeginLifetimeScope();
        }
    }
}