using System.Windows;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.Gui.Wpf.Native.Services
{
    public class ClipboardManager : IClipboardManager
    {
        public void SetText(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch { }
        }
    }
}
