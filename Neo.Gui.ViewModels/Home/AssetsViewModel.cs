using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.VM;

using Neo.Gui.Base.Collections;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Helpers;
using Neo.Gui.Base.Managers;

namespace Neo.Gui.ViewModels.Home
{
    public class AssetsViewModel : 
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<ClearAssetsMessage>,
        IMessageHandler<AssetAddedMessage>
    {
        #region Private Fields 
        private static readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();

        private readonly IDialogManager dialogManager;
        private readonly IProcessHelper processHelper;
        private readonly ISettingsManager settingsManager;
        private readonly IWalletController walletController;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IMessagePublisher messagePublisher;
        private AssetItem selectedAsset;
        #endregion

        #region Public Properties
        public ConcurrentObservableCollection<AssetItem> Assets { get; private set; }

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
                if (this.SelectedAsset == null) return false;

                if (this.SelectedAsset.IsSystemAsset) return false;

                if (this.SelectedAsset.State?.Owner == null) return false;

                return this.walletController.CanViewCertificate(this.SelectedAsset);
            }
        }

        public bool DeleteAssetEnabled => 
            this.SelectedAsset != null &&
            (this.SelectedAsset.State == null ||
            (this.SelectedAsset.State.AssetType != AssetType.GoverningToken &&
            this.SelectedAsset.State.AssetType != AssetType.UtilityToken));

        public RelayCommand ViewCertificateCommand => new RelayCommand(this.ViewCertificate);

        public RelayCommand DeleteAssetCommand => new RelayCommand(this.DeleteAsset);

        public RelayCommand ViewSelectedAssetDetailsCommand => new RelayCommand(this.ViewSelectedAssetDetails);
        #endregion Properties

        #region Constructor 
        public AssetsViewModel(
            IDialogManager dialogManager,
            IProcessHelper processHelper,
            ISettingsManager settingsManager,
            IWalletController walletController,
            IMessageSubscriber messageSubscriber,
            IMessagePublisher messagePublisher)
        {
            this.dialogManager = dialogManager;
            this.processHelper = processHelper;
            this.settingsManager = settingsManager;
            this.walletController = walletController;
            this.messageSubscriber = messageSubscriber;
            this.messagePublisher = messagePublisher;

            this.Assets = new ConcurrentObservableCollection<AssetItem>();
        }
        #endregion

        #region ILoadable implementation
        public void OnLoad(params object[] parameters)
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
        public void HandleMessage(ClearAssetsMessage message)
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

            var assetURLFormat = this.settingsManager.AddressURLFormat;
            var url = string.Format(assetURLFormat, this.SelectedAsset.Name.Substring(2));

            this.processHelper.OpenInExternalBrowser(url);
        }

        private void ViewCertificate()
        {
            if (!this.ViewCertificateEnabled) return;
            
            var certificatePath = this.walletController.ViewCertificate(this.SelectedAsset);

            if (string.IsNullOrEmpty(certificatePath))
            {
                // TODO Show error message
            }
            else
            {
                this.processHelper.Run(certificatePath);
            }
        }

        private void DeleteAsset()
        {
            if (this.SelectedAsset?.State == null) return;

            var value = this.walletController.GetAvailable(this.SelectedAsset.State.AssetId);

            var result = this.dialogManager.ShowMessageDialog(
                Strings.DeleteConfirmation,
                $"{Strings.DeleteAssetConfirmationMessage}\n{string.Join("\n", $"{this.SelectedAsset.State.GetName()}:{value}")}",
                MessageDialogType.YesNo,
                MessageDialogResult.No);

            if (result != MessageDialogResult.Yes) return;

            this.walletController.DeleteAsset(this.SelectedAsset);
        }
        #endregion
    }
}
