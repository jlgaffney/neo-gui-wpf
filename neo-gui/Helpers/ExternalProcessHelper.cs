using System.Diagnostics;
using Neo.Gui.Helpers.Interfaces;

namespace Neo.Helpers
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
