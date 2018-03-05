using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.UI.Core.Globalization.Resources;
using Neo.Gui.Dialogs;
using Neo.UI.Core.Data;
using Neo.UI.Core.Messaging.Interfaces;
using System.Linq;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.ViewModels.Data;
using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Wallet;
using Neo.UI.Core.Wallet.Messages;

namespace Neo.Gui.ViewModels.Home
{
    public class AssetsViewModel : 
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<AssetAddedMessage>
    {
        #region Private Fields 
        private readonly IDialogManager dialogManager;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IProcessManager processManager;
        private readonly ISettingsManager settingsManager;
        private readonly IWalletController walletController;

        private UiAssetSummary selectedAsset;
        #endregion

        #region Public Properties
        public ObservableCollection<UiAssetSummary> Assets { get; }

        public UiAssetSummary SelectedAsset
        {
            get => this.selectedAsset;
            set
            {
                if (this.selectedAsset == value) return;

                this.selectedAsset = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.ViewCertificateEnabled));
                RaisePropertyChanged(nameof(this.DeleteAssetEnabled));
            }
        }

        public bool ViewCertificateEnabled
        {
            get
            {
                if (this.SelectedAsset == null) return false;

                if (this.SelectedAsset.TokenType != TokenType.FirstClassToken) return false;

                return this.walletController.CanViewCertificate(this.SelectedAsset.AssetId);
            }
        }

        // TODO Should this also check if the user issued the asset?
        public bool DeleteAssetEnabled => 
            this.SelectedAsset != null &&
            this.SelectedAsset.TokenType == TokenType.FirstClassToken &&
            !this.SelectedAsset.IsSystemAsset;

        public RelayCommand ViewCertificateCommand => new RelayCommand(this.ViewCertificate);

        public RelayCommand DeleteAssetCommand => new RelayCommand(this.DeleteAsset);

        public RelayCommand ViewSelectedAssetDetailsCommand => new RelayCommand(this.ViewSelectedAssetDetails);
        #endregion Properties

        #region Constructor 
        public AssetsViewModel(
            IDialogManager dialogManager,
            IMessageSubscriber messageSubscriber,
            IProcessManager processManager,
            ISettingsManager settingsManager,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.messageSubscriber = messageSubscriber;
            this.processManager = processManager;
            this.settingsManager = settingsManager;
            this.walletController = walletController;

            this.Assets = new ObservableCollection<UiAssetSummary>();
        }
        #endregion

        #region ILoadable implementation
        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);
        }
        #endregion

        #region IUnloadable implementation
        public void OnUnload()
        {
            this.messageSubscriber.Unsubscribe(this);
        }
        #endregion

        #region IMessageHandler implementation
        public void HandleMessage(CurrentWalletHasChangedMessage message)
        {
            this.Assets.Clear();
        }

        public void HandleMessage(AssetAddedMessage message)
        {
            var isAssetInTheList = this.Assets.Any(x => x.AssetId == message.AssetId);

            if (isAssetInTheList)
            {
                var assetInList = this.Assets.Single(x => x.AssetId == message.AssetId);
                
            }
            else
            {
                var newAsset = new UiAssetSummary(message.AssetId, message.AssetName, message.AssetIssuer, message.Type, message.TokenType, message.IsSystemAsset, message.TotalBalance);

                this.Assets.Add(newAsset);
            }
        }
        #endregion

        #region Private Methods 
        private void ViewSelectedAssetDetails()
        {
            if (this.SelectedAsset == null) return;

            var assetId = this.SelectedAsset.AssetId;
            if (assetId.StartsWith("0x"))
            {
                assetId = assetId.Substring(2);
            }

            var url = string.Format(this.settingsManager.AssetURLFormat, assetId);

            this.processManager.OpenInExternalBrowser(url);
        }

        private void ViewCertificate()
        {
            if (!this.ViewCertificateEnabled) return;
            
            var certificatePath = this.walletController.ViewCertificate(this.SelectedAsset.AssetId);

            if (string.IsNullOrEmpty(certificatePath))
            {
                // TODO Show error message
            }
            else
            {
                this.processManager.Run(certificatePath);
            }
        }

        private void DeleteAsset()
        {
            if (this.SelectedAsset == null || this.SelectedAsset.TokenType != TokenType.FirstClassToken) return;

            var result = this.dialogManager.ShowMessageDialog(
                Strings.DeleteConfirmation,
                $"{Strings.DeleteAssetConfirmationMessage}\n{string.Join("\n", $"{this.SelectedAsset.Name}:{this.SelectedAsset.TotalBalance}")}",
                MessageDialogType.YesNo,
                MessageDialogResult.No);

            if (result != MessageDialogResult.Yes) return;

            this.walletController.DeleteFirstClassAsset(this.SelectedAsset.AssetId);
        }
        #endregion
    }
}
