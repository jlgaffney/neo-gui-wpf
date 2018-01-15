using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Updater;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.Wpf.Properties;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.Gui.Wpf.Views.Updater
{
    /// <summary>
    /// View model for updating the application. NOTE this view model is Windows-specific,
    /// and is only capable of updating the application on the Windows platform.
    /// </summary>
    /// <remarks>
    /// DO NOT move this view model out of the WPF project unless the Windows-specific
    /// application updating logic has been abstracted out of this view model.
    /// </remarks>
    public class UpdateViewModel : ViewModelBase, IDialogViewModel<UpdateLoadParameters>
    {
        private const string OfficialWebsiteUrl = "https://neo.org/";

        private const string UpdateFileName = "update.bat";
        private const string DownloadPath = "update.zip";

        private readonly WebClient web = new WebClient();

        private readonly ICompressedFileManager compressedFileManager;
        private readonly IDirectoryManager directoryManager;
        private readonly IFileManager fileManager;
        private readonly IProcessManager processManager;

        private readonly Version latestVersion;
        private readonly string downloadUrl;

        private int updateDownloadProgress;

        private bool buttonsEnabled;

        public UpdateViewModel(
            ICompressedFileManager compressedFileManager,
            IDirectoryManager directoryManager,
            IFileManager fileManager,
            IProcessManager processManager,
            IVersionService versionService)
        {
            this.compressedFileManager = compressedFileManager;
            this.directoryManager = directoryManager;
            this.fileManager = fileManager;
            this.processManager = processManager;

            // Setup update information
            this.latestVersion = versionService.LatestVersion;

            var latestReleaseInfo = versionService.GetLatestReleaseInfo();

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

                RaisePropertyChanged();
            }
        }

        public bool ButtonsEnabled
        {
            get => this.buttonsEnabled;
            set
            {
                if (this.buttonsEnabled == value) return;

                this.buttonsEnabled = value;

                RaisePropertyChanged();
            }
        }

        public ICommand GoToOfficialWebsiteCommand => new RelayCommand(GoToOfficialWebsite);

        public ICommand GoToDownloadPageCommand => new RelayCommand(this.GoToDownloadPage);

        public ICommand UpdateCommand => new RelayCommand(this.Update);

        public ICommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(UpdateLoadParameters parameters)
        {
        }
        #endregion

        private void GoToOfficialWebsite()
        {
            this.processManager.OpenInExternalBrowser(OfficialWebsiteUrl);
        }

        private void GoToDownloadPage()
        {
            this.processManager.OpenInExternalBrowser(this.downloadUrl);
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

            const string update1DirectoryPath = "update";
            const string update2DirectoryPath = "update2";

            // Delete update directory if it exists
            if (this.directoryManager.DirectoryExists(update1DirectoryPath))
            {
                this.directoryManager.Delete(update1DirectoryPath);
            }

            // Create update directory
            this.directoryManager.Create(update1DirectoryPath);

            // Extract update zip file to directory
            this.compressedFileManager.ExtractZipFileToDirectory(DownloadPath, update1DirectoryPath);

            var updateSubDirectories = this.directoryManager.GetSubDirectories(update1DirectoryPath);

            if (updateSubDirectories.Length == 1)
            {
                this.directoryManager.Move(updateSubDirectories[0], update2DirectoryPath);

                this.directoryManager.Delete(update1DirectoryPath);

                this.directoryManager.Move(update2DirectoryPath, update1DirectoryPath);
            }

            this.fileManager.WriteAllBytes(UpdateFileName, Resources.UpdateBat);

            // Update application
            this.processManager.Run(UpdateFileName);

            this.processManager.Exit();

            this.Close(this, EventArgs.Empty);
        }
        #endregion Update Downloader Methods
    }
}