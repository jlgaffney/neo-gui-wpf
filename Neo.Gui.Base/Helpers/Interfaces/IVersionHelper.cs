using System;
using Neo.Gui.Base.Updating;

namespace Neo.Gui.Base.Interfaces.Helpers
{
    public interface IVersionHelper
    {
        Version CurrentVersion { get; }

        Version LatestVersion { get; }

        bool UpdateIsRequired { get; }

        ReleaseInfo GetLatestReleaseInfo();
    }
}
