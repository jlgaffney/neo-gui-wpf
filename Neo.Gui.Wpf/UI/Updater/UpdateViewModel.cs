using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Input;
using Neo.Gui.Base.Interfaces.Helpers;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;
using NeoResources = Neo.Gui.Wpf.Properties.Resources;

namespace Neo.UI.Updater
{
    public class UpdateViewModel : ViewModelBase
    {
        private const string UpdateFileName = "update.bat";
        private const string DownloadPath = "update.zip";

        private readonly WebClient web = new WebClient();

        private readonly IExternalProcessHelper externalProcessHelper;
        private readonly IMessagePublisher messagePublisher;

        private readonly Version latestVersion;
        private readonly string downloadUrl;

        private int updateDownloadProgress;

        private bool buttonsEnabled;

        public UpdateViewModel(
            IExternalProcessHelper externalProcessHelper,
            IVersionHelper versionHelper,
            IMessagePublisher messagePublisher)
        {
            this.externalProcessHelper = externalProcessHelper;
            this.messagePublisher = messagePublisher;

            // Setup update information
            this.latestVersion = versionHelper.LatestVersion;

            var latestReleaseInfo = versionHelper.GetLatestReleaseInfo();

            if (latestReleaseInfo == null) return;

            this.downloadUrl = latestReleaseInfo.DownloadUrl;
            this.Changes = latestReleaseInfo.Changes;
            
            this.web.DownloadProgressChanged += WebDownloadProgressChanged;
            this.web.DownloadFileCompleted += WebDownloadFileCompleted;

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

        private void GoToOfficialWebsite()
        {
            this.externalProcessHelper.OpenInExternalBrowser("https://neo.org/");
        }

        private void GoToDownloadPage()
        {
            this.externalProcessHelper.OpenInExternalBrowser(this.downloadUrl);
        }

        #region Update Downloader Methods

        private void Update()
        {
            this.ButtonsEnabled = false;
            
            web.DownloadFileAsync(new Uri(this.downloadUrl), DownloadPath);
        }

        private void WebDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.UpdateDownloadProgress = e.ProgressPercentage;
        }

        private void WebDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null) return;

            var directoryInfo = new DirectoryInfo("update");

            // Delete update directory if it exists
            if (directoryInfo.Exists) directoryInfo.Delete(true);

            // Create update directory
            directoryInfo.Create();

            // Extract update zip file to directory
            ZipFile.ExtractToDirectory(DownloadPath, directoryInfo.Name);

            var fileSystemInfo = directoryInfo.GetFileSystemInfos();
            if (fileSystemInfo.Length == 1 && fileSystemInfo[0] is DirectoryInfo)
            {
                ((DirectoryInfo) fileSystemInfo[0]).MoveTo("update2");

                directoryInfo.Delete();

                Directory.Move("update2", directoryInfo.Name);
            }

            File.WriteAllBytes(UpdateFileName, NeoResources.UpdateBat);

            this.TryClose();

            // Update application
            this.messagePublisher.Publish(new UpdateApplicationMessage(UpdateFileName));
        }

        #endregion Update Downloader Methods
    }
}