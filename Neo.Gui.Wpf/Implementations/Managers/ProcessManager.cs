using System.Diagnostics;
using System.Windows;

using Neo.Gui.Base.Managers.Interfaces;

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

        public void Restart()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
        #endregion
    }
}
