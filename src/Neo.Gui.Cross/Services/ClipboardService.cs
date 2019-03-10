using TextCopy;

namespace Neo.Gui.Cross.Services
{
    public class ClipboardService : IClipboardService
    {
        public bool SetText(string value)
        {
            try
            {
                Clipboard.SetText(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
