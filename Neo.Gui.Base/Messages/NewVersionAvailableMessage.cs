using System;

namespace Neo.Gui.Base.Messages
{
    public class NewVersionAvailableMessage
    {
        public NewVersionAvailableMessage(Version newVersion)
        {
            this.NewVersion = newVersion;
        }

        public Version NewVersion { get; }
    }
}
