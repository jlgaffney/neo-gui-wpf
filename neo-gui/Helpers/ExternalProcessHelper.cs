using System;
using System.Diagnostics;
using System.Reflection;

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
