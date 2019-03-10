using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using CommonServiceLocator;
using Neo.Gui.Cross.Controllers;

namespace Neo.Gui.Cross
{
    public class App : Application
    {
        private readonly IApplicationController applicationController;

        public App()
        {
            this.applicationController = ServiceLocator.Current.GetInstance<IApplicationController>();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            this.applicationController.Start();
        }

        protected override void OnExiting(object sender, EventArgs e)
        {
            this.applicationController.Stop();
        }
    }
}
