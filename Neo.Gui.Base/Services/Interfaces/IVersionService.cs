using System;

using Neo.Gui.Base.Updating;

namespace Neo.Gui.Base.Services.Interfaces
{
    public interface IVersionService
    {
        Version CurrentVersion { get; }

        Version LatestVersion { get; }

        bool UpdateIsRequired { get; }

        ReleaseInfo GetLatestReleaseInfo();
    }
}
