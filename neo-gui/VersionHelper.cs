using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Neo
{
    public static class VersionHelper
    {
        #region Update XML Document

        // Used to prevent multiple fails load attempts
        private static bool xmlLoadAttempted;
        private static XDocument updateXml;


        private static XDocument UpdateXmlDocument
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


        public static bool UpdateIsRequired => CurrentVersion < MinimumVersion;

        public static Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

        public static Version MinimumVersion
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

        public static Version LatestVersion
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

        public static bool GetLatestReleaseInfo(out string changes, out string downloadUrl)
        {
            changes = null;
            downloadUrl = null;

            if (UpdateXmlDocument == null) return false;

            var latestVersionStr = LatestVersion.ToString();

            var updateElement = UpdateXmlDocument.Element("update");

            if (updateElement == null) return false;

            var releaseElements = updateElement.Elements("release");

            var latestRelease = releaseElements.FirstOrDefault(releaseElement =>
            {
                var versionAttribute = releaseElement.Attribute("version");

                if (versionAttribute == null) return false;

                return versionAttribute.Value == latestVersionStr;
            });

            if (latestRelease == null) return false;

            var changeElement = latestRelease.Element("changes");
            var downloadUrlElement = latestRelease.Attribute("file");

            if (changeElement == null && downloadUrlElement == null) return false;

            changes = changeElement?.Value.Replace("\n", Environment.NewLine) ?? string.Empty;
            downloadUrl = downloadUrlElement?.Value ?? string.Empty;

            return true;
        }
    }
}