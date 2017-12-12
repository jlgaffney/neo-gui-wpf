using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Updating;

namespace Neo.Gui.Wpf.Implementations.Helpers
{
    public class VersionHelper : IVersionHelper
    {
        #region Update XML Document

        // Used to prevent multiple fails load attempts
        private bool xmlLoadAttempted;
        private XDocument updateXml;


        private XDocument UpdateXmlDocument
        {
            get
            {
                if (updateXml != null) return updateXml;

                // Check if an attempt has already been made to load update XML
                if (xmlLoadAttempted) return null;

                // Try to load update xml document
                try
                {
                    updateXml = XDocument.Load("https://neo.org/client/update.xml");
                    return updateXml;
                }
                catch
                {
                    return null;
                }
                finally
                {
                    xmlLoadAttempted = true;
                }
            }
        }

        #endregion Update XML Document

        #region IVersionHelper implementation

        public Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

        public Version LatestVersion
        {
            get
            {
                var latestVersionStr = UpdateXmlDocument?.Element("update")?.Attribute("latest")?.Value;

                if (string.IsNullOrEmpty(latestVersionStr)) return null;

                if (Version.TryParse(latestVersionStr, out var latestVersion))
                {
                    return latestVersion;
                }

                return null;
            }
        }

        public bool UpdateIsRequired => CurrentVersion < MinimumVersion;
        
        public ReleaseInfo GetLatestReleaseInfo()
        {
            if (UpdateXmlDocument == null) return null;

            var latestVersion = LatestVersion;

            if (latestVersion == null) return null;

            var latestVersionStr = latestVersion.ToString();

            var updateElement = UpdateXmlDocument.Element("update");

            if (updateElement == null) return null;

            var releaseElements = updateElement.Elements("release");

            var latestRelease = releaseElements.FirstOrDefault(releaseElement =>
            {
                var versionAttribute = releaseElement.Attribute("version");

                if (versionAttribute == null) return false;

                return versionAttribute.Value == latestVersionStr;
            });

            if (latestRelease == null) return null;

            var changeElement = latestRelease.Element("changes");
            var downloadUrlElement = latestRelease.Attribute("file");

            if (changeElement == null && downloadUrlElement == null) return null;

            var downloadUrl = downloadUrlElement?.Value ?? string.Empty;
            var changes = changeElement?.Value.Replace("\n", Environment.NewLine) ?? string.Empty;

            var latestReleaseInfo = new ReleaseInfo(downloadUrl, changes);
            
            return latestReleaseInfo;
        }

        #endregion

        #region Private methods
        
        private Version MinimumVersion
        {
            get
            {
                var minimumVersionStr = UpdateXmlDocument?.Element("update")?.Attribute("minimum")?.Value;

                if (string.IsNullOrEmpty(minimumVersionStr)) return null;

                if (Version.TryParse(minimumVersionStr, out var minimumVersion))
                {
                    return minimumVersion;
                }

                return null;
            }
        }

        #endregion
    }
}