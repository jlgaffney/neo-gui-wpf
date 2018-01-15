using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Globalization.Resources;
using Neo.Gui.Dialogs;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Messages;
using Neo.UI.Core.Messaging.Interfaces;

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

        private AssetItem selectedAsset;
        #endregion

        #region Public Properties
        public ObservableCollection<AssetItem> Assets { get; }

        public AssetItem SelectedAsset
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
                var selectedFirstClassAsset = this.SelectedAsset as FirstClassAssetItem;

                if (selectedFirstClassAsset == null) return false;

                if (selectedFirstClassAsset.IsSystemAsset) return false;

                if (selectedFirstClassAsset.AssetOwner == null) return false;

                return this.walletController.CanViewCertificate(selectedFirstClassAsset);
            }
        }

        // TODO Should this also check if the user issued the asset?
        public bool DeleteAssetEnabled => 
            this.SelectedAsset != null &&
            this.SelectedAsset is FirstClassAssetItem &&
            !((FirstClassAssetItem)this.SelectedAsset).IsSystemAsset;

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

            this.Assets = new ObservableCollection<AssetItem>();
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
            this.Assets.Add(message.Asset);
        }
        #endregion

        #region Private Methods 
        private void ViewSelectedAssetDetails()
        {
            if (this.SelectedAsset == null) return;

            var url = string.Format(this.settingsManager.AssetURLFormat, this.SelectedAsset.Name.Substring(2));

            this.processManager.OpenInExternalBrowser(url);
        }

        private void ViewCertificate()
        {
            if (!this.ViewCertificateEnabled) return;
            
            var certificatePath = this.walletController.ViewCertificate(this.SelectedAsset as FirstClassAssetItem);

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
            var firstClassAssetItem = this.SelectedAsset as FirstClassAssetItem;

            if (firstClassAssetItem == null) return;

            var value = this.walletController.GetAvailable(firstClassAssetItem.AssetId);

            var result = this.dialogManager.ShowMessageDialog(
                Strings.DeleteConfirmation,
                $"{Strings.DeleteAssetConfirmationMessage}\n{string.Join("\n", $"{firstClassAssetItem.Name}:{value}")}",
                MessageDialogType.YesNo,
                MessageDialogResult.No);

            if (result != MessageDialogResult.Yes) return;

            this.walletController.DeleteFirstClassAsset(firstClassAssetItem);
        }
        #endregion
    }
}
