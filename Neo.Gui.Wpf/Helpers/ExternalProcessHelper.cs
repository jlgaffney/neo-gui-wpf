using System.Diagnostics;
using Neo.Gui.Base.Helpers.Interfaces;

namespace Neo.Gui.Wpf.Helpers
{
    public class ExternalProcessHelper : IExternalProcessHelper
    {
        #region IExternalProcessHelper implementation 
        public void OpenInExternalBrowser(string url)
        {
            Process.Start(url);
        }
        #endregion
    }
}
