using System.Diagnostics;
using Neo.Gui.Base.Interfaces.Helpers;

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
