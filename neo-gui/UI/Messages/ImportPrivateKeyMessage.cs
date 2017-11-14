using System.Collections.Generic;

namespace Neo.UI.Messages
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