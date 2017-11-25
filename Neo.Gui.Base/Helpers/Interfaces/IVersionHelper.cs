using System;
using Neo.Gui.Base.Updating;

namespace Neo.Gui.Base.Helpers.Interfaces
{
    public interface IVersionHelper
    {
        Version CurrentVersion { get; }

        Version LatestVersion { get; }

        bool UpdateIsRequired { get; }

        ReleaseInfo GetLatestReleaseInfo();
    }
}
