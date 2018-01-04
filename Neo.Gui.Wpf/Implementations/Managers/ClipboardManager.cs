using System.Windows;

using Neo.Gui.Base.Managers.Interfaces;

namespace Neo.Gui.Wpf.Implementations.Managers
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
