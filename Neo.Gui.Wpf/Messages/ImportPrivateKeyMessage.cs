using System.Collections.Generic;

namespace Neo.Gui.Wpf.Messages
{
    public class ImportPrivateKeyMessage
    {
        public List<string> WifStrings { get; }

        public ImportPrivateKeyMessage(List<string> wifStrings)
        {
            this.WifStrings = wifStrings;
        }
    }
}