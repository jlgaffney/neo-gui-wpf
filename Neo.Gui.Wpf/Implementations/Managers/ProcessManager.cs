using System.Diagnostics;
using System.Windows;

using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Managers.Interfaces;

namespace Neo.Gui.Wpf.Implementations.Managers
{
    public class ProcessManager : IProcessManager
    {
        #region IProcessHelper implementation 
        public void Run(string path)
        {
            Process.Start(path);
        }

        public void OpenInExternalBrowser(string url)
        {
            Process.Start(url);
        }

        public void Exit()
        {
            Application.Current.Shutdown();
        }

        public void Restart()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
        #endregion
    }
}
