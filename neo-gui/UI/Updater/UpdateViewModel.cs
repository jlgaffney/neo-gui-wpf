using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Input;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;
using NeoResources = Neo.Properties.Resources;

namespace Neo.UI.Updater
{
    public class UpdateViewModel : ViewModelBase
    {
        private const string downloadPath = "update.zip";

        private readonly WebClient web = new WebClient();

        private readonly Version latestVersion;
        private readonly string downloadUrl;

        private int updateDownloadProgress;

        private bool buttonsEnabled;

        public UpdateViewModel()
        {
            // Setup update information
            this.latestVersion = VersionHelper.LatestVersion;

            VersionHelper.GetLatestReleaseInfo(out var releaseChanges, out var releaseDownloadUrl);
            this.Changes = releaseChanges;
            this.downloadUrl = releaseDownloadUrl;
            
            web.DownloadProgressChanged += Web_DownloadProgressChanged;
            web.DownloadFileCompleted += Web_DownloadFileCompleted;

            this.ButtonsEnabled = true;
        }

        public string LatestVersionValue => this.latestVersion.ToString();

        public string Changes { get; }

        public int UpdateDownloadProgress
        {
            get => this.updateDownloadProgress;
            set
            {
                if (this.updateDownloadProgress == value) return;

                this.updateDownloadProgress = value;

                NotifyPropertyChanged();
            }
        }

        public bool ButtonsEnabled
        {
            get => this.buttonsEnabled;
            set
            {
                if (this.buttonsEnabled == value) return;

                this.buttonsEnabled = value;

                NotifyPropertyChanged();
            }
        }

        public ICommand GoToOfficialWebsiteCommand => new RelayCommand(GoToOfficialWebsite);

        public ICommand GoToDownloadPageCommand => new RelayCommand(this.GoToDownloadPage);

        public ICommand UpdateCommand => new RelayCommand(this.Update);

        public ICommand CloseCommand => new RelayCommand(this.TryClose);

        private static void GoToOfficialWebsite()
        {
            Process.Start("https://neo.org/");
        }

        private void GoToDownloadPage()
        {
            Process.Start(this.downloadUrl);
        }

        #region Update Downloader Methods

        private void Update()
        {
            this.ButtonsEnabled = false;
            
            web.DownloadFileAsync(new Uri(this.downloadUrl), downloadPath);
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