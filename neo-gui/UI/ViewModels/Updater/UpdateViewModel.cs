using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows.Input;
using System.Xml.Linq;
using Neo.UI.Controls;
using Neo.UI.Messages;
using Neo.UI.MVVM;
using NeoResources = Neo.Properties.Resources;

namespace Neo.UI.ViewModels.Updater
{
    public class UpdateViewModel : ViewModelBase
    {
        private readonly WebClient web = new WebClient();
        private string downloadUrl;
        private string downloadPath;

        private Version latestVersion;
        private string changes;

        private int updateDownloadProgress;

        private bool buttonsEnabled;

        public string LatestVersionValue => this.latestVersion.ToString();

        public string Changes
        {
            get { return this.changes; }
            set
            {
                if (this.changes == value) return;

                this.changes = value;

                NotifyPropertyChanged();
            }
        }

        public int UpdateDownloadProgress
        {
            get { return this.updateDownloadProgress; }
            set
            {
                if (this.updateDownloadProgress == value) return;

                this.updateDownloadProgress = value;

                NotifyPropertyChanged();
            }
        }

        public bool ButtonsEnabled
        {
            get { return this.buttonsEnabled; }
            set
            {
                if (this.buttonsEnabled == value) return;

                this.buttonsEnabled = value;

                NotifyPropertyChanged();
            }
        }

        public ICommand GoToOfficialWebsiteCommand => new RelayCommand(this.GoToOfficialWebsite);

        public ICommand GoToDownloadPageCommand => new RelayCommand(this.GoToDownloadPage);

        public ICommand UpdateCommand => new RelayCommand(this.Update);

        private void GoToOfficialWebsite()
        {
            Process.Start("https://neo.org/");
        }

        private void GoToDownloadPage()
        {
            Process.Start(this.downloadUrl);
        }

        #region Update Downloader Methods

        public void SetUpdateInfo(XDocument newVersionXml)
        {
            Debug.Assert(this.latestVersion != null);

            var latest = Version.Parse(newVersionXml.Element("update").Attribute("latest").Value);

            this.latestVersion = latest;

            var release = newVersionXml.Element("update").Elements("release").First(p => p.Attribute("version").Value == latest.ToString());

            this.Changes = release.Element("changes").Value.Replace("\n", Environment.NewLine);
            this.downloadUrl = release.Attribute("file").Value;

            web.DownloadProgressChanged += Web_DownloadProgressChanged;
            web.DownloadFileCompleted += Web_DownloadFileCompleted;

            this.ButtonsEnabled = true;
        }

        private void Update()
        {
            this.ButtonsEnabled = false;
            
            this.downloadPath = "update.zip";
            web.DownloadFileAsync(new Uri(this.downloadUrl), this.downloadPath);
        }

        private void Web_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.UpdateDownloadProgress = e.ProgressPercentage;
        }

        private void Web_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null) return;

            var directoryInfo = new DirectoryInfo("update");

            // Delete update directory if it exists
            if (directoryInfo.Exists) directoryInfo.Delete(true);

            // Create update directory
            directoryInfo.Create();

            // Extract update zip file to directory
            ZipFile.ExtractToDirectory(downloadPath, directoryInfo.Name);

            var fileSystemInfo = directoryInfo.GetFileSystemInfos();
            if (fileSystemInfo.Length == 1 && fileSystemInfo[0] is DirectoryInfo)
            {
                ((DirectoryInfo) fileSystemInfo[0]).MoveTo("update2");

                directoryInfo.Delete();

                Directory.Move("update2", directoryInfo.Name);
            }

            File.WriteAllBytes("update.bat", NeoResources.UpdateBat);

            this.TryClose();

            // Update application
            EventAggregator.Current.Publish(new UpdateApplicationMessage("update.bat"));
        }

        #endregion Update Downloader Methods
    }
}